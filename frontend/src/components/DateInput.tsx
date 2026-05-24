import React from 'react';
import { UseFormRegisterReturn } from 'react-hook-form';
import ValidationMessage from './ValidationMessage';

interface DateInputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  label: string;
  error?: string;
  register?: UseFormRegisterReturn;
}

export const DateInput: React.FC<DateInputProps> = ({
  label,
  error,
  register,
  id,
  required,
  ...props
}) => {
  const errorId = error ? `${id}-error` : undefined;

  return (
    <div className="input-field-container">
      <label htmlFor={id} className="input-field-label">
        {label}
        {required && <span className="required-indicator"> *</span>}
      </label>
      <input
        type="date"
        id={id}
        className={`input-field-text ${error ? 'input-field-error' : ''}`}
        aria-invalid={error ? 'true' : 'false'}
        aria-describedby={errorId}
        {...register}
        {...props}
      />
      <ValidationMessage message={error} id={errorId} />
    </div>
  );
};

export default DateInput;
