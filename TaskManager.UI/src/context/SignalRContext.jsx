import { createContext, useContext, useEffect, useState } from 'react';
import * as signalR from '@microsoft/signalr';
import { useAuth } from './AuthContext';

const SignalRContext = createContext(null);

export function SignalRProvider({ children }) {
  const { isAuthenticated } = useAuth();
  const [connection, setConnection] = useState(null);
  const [notification, setNotification] = useState(null);

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
      newConnection.stop();
    };
  }, [isAuthenticated]);

  const showNotification = (msg) => {
    setNotification(msg);
    setTimeout(() => {
      setNotification(null);
    }, 4000);
  };

  return (
    <SignalRContext.Provider value={{ connection, notification }}>
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
