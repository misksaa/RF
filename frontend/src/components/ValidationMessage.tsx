import React from 'react';

interface ValidationMessageProps {
  message?: string;
  id?: string;
}

export const ValidationMessage: React.FC<ValidationMessageProps> = ({ message, id }) => {
  if (!message) return null;
  return (
    <span className="validation-error-message" id={id} role="alert">
      {message}
    </span>
  );
};

export default ValidationMessage;
