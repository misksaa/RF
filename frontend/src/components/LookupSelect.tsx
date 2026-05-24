import React from 'react';
import type { UseFormRegisterReturn } from 'react-hook-form';
import type { LookupDto } from '../types';
import ValidationMessage from './ValidationMessage';

interface LookupSelectProps extends React.SelectHTMLAttributes<HTMLSelectElement> {
  label: string;
  options: LookupDto[];
  error?: string;
  register?: UseFormRegisterReturn;
  loading?: boolean;
  placeholderText?: string;
}

export const LookupSelect: React.FC<LookupSelectProps> = ({
  label,
  options,
  error,
  register,
  id,
  required,
  loading,
  placeholderText = "Select...",
  ...props
}) => {
  const errorId = error ? `${id}-error` : undefined;

  return (
    <div className="input-field-container">
      <label htmlFor={id} className="input-field-label">
        {label}
        {required && <span className="required-indicator"> *</span>}
      </label>
      <select
        id={id}
        className={`input-field-select ${error ? 'input-field-error' : ''}`}
        aria-invalid={error ? 'true' : 'false'}
        aria-describedby={errorId}
        disabled={loading || props.disabled}
        {...register}
        {...props}
      >
        <option value="">{loading ? 'Loading...' : placeholderText}</option>
        {options.map((opt) => (
          <option key={opt.id} value={opt.id}>
            {opt.nameEn} / {opt.nameAr}
          </option>
        ))}
      </select>
      <ValidationMessage message={error} id={errorId} />
    </div>
  );
};

export default LookupSelect;
