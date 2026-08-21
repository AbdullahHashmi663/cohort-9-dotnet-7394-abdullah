import { createContext, useContext, useEffect, useState, useMemo, useCallback, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
import { useAuth } from './AuthContext';

const SignalRContext = createContext(null);

export function SignalRProvider({ children }) {
  const { isAuthenticated } = useAuth();
  const [connection, setConnection] = useState(null);
  const [notification, setNotification] = useState(null);
  const timerRef = useRef(null);

  const showNotification = useCallback((msg) => {
    if (timerRef.current) {
      clearTimeout(timerRef.current);
    }
    setNotification(msg);
    timerRef.current = setTimeout(() => {
      setNotification(null);
    }, 4000);
  }, []);

  useEffect(() => {
    if (!isAuthenticated) {
      if (connection) {
        connection.stop();
        setConnection(null);
      }
      return;
    }

    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/task')
      .withAutomaticReconnect()
      .build();

    newConnection
      .start()
      .then(() => {
        setConnection(newConnection);

        newConnection.on('TaskCreated', (task) => {
          showNotification(`✨ New Task Created: "${task.title}"`);
        });

        newConnection.on('TaskUpdated', (task) => {
          showNotification(`🔄 Task Updated: "${task.title}"`);
        });

        newConnection.on('TaskDeleted', (taskId) => {
          showNotification(`🗑️ Task ID #${taskId} was deleted`);
        });
      })
      .catch((err) => console.log('SignalR Connection Error: ', err));

    return () => {
      if (timerRef.current) {
        clearTimeout(timerRef.current);
      }
      newConnection.stop();
    };
  }, [isAuthenticated, showNotification]);

  const value = useMemo(() => ({ connection, notification }), [connection, notification]);

  return (
    <SignalRContext.Provider value={value}>
      {children}
      {notification && (
        <div className="toast-notification">
          <span>{notification}</span>
        </div>
      )}
    </SignalRContext.Provider>
  );
}

export function useSignalR() {
  return useContext(SignalRContext);
}
