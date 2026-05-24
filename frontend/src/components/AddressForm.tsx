import React from 'react';
import type { UseFormRegister, FieldErrors } from 'react-hook-form';
import type { LookupDto, RegistrationDto } from '../types';
import TextInput from './TextInput';
import LookupSelect from './LookupSelect';
import { Trash2, ShieldCheck, MapPin } from 'lucide-react';

interface AddressFormProps {
  index: number;
  register: UseFormRegister<RegistrationDto>;
  errors: FieldErrors<RegistrationDto>;
  governorates: LookupDto[];
  cities: LookupDto[];
  loadingCities: boolean;
  canRemove: boolean;
  onRemove: () => void;
  onGovernorateChange: (governorateId: number) => void;
  onMakePrimary: () => void;
  isPrimary: boolean;
}

export const AddressForm: React.FC<AddressFormProps> = ({
  index,
  register,
  errors,
  governorates,
  cities,
  loadingCities,
  canRemove,
  onRemove,
  onGovernorateChange,
  onMakePrimary,
  isPrimary,
}) => {
  const addressErrors = errors.addresses?.[index] as any;

  return (
    <div className={`address-card ${isPrimary ? 'address-primary-active' : ''}`}>
      <div className="address-card-header">
        <div className="address-title">
          <MapPin className="address-icon" size={18} />
          <h3>Address #{index + 1}</h3>
          {isPrimary && <span className="primary-badge">Primary</span>}
        </div>
        <div className="address-controls">
          {!isPrimary && (
            <button
              type="button"
              className="btn-make-primary"
              onClick={onMakePrimary}
              title="Set as Primary"
            >
              <ShieldCheck size={16} /> Set Primary
            </button>
          )}
          {canRemove && (
            <button
              type="button"
              className="btn-remove-address"
              onClick={onRemove}
              title="Remove Address"
            >
              <Trash2 size={16} /> Remove
            </button>
          )}
        </div>
      </div>

      <div className="address-card-grid">
        <LookupSelect
          label="Governorate"
          id={`addresses-${index}-governorate`}
          options={governorates}
          error={addressErrors?.governorateId?.message}
          required
          register={register(`addresses.${index}.governorateId`, {
            valueAsNumber: true,
            onChange: (e) => {
              const val = parseInt(e.target.value, 10);
              onGovernorateChange(isNaN(val) ? 0 : val);
            }
          })}
        />

        <LookupSelect
          label="City"
          id={`addresses-${index}-city`}
          options={cities}
          error={addressErrors?.cityId?.message}
          required
          loading={loadingCities}
          disabled={!cities.length}
          placeholderText={cities.length ? "Select City..." : "Select Governorate first..."}
          register={register(`addresses.${index}.cityId`, {
            valueAsNumber: true
          })}
        />

        <TextInput
          label="Street"
          id={`addresses-${index}-street`}
          placeholder="e.g. 15 Tahrir St."
          error={addressErrors?.street?.message}
          required
          register={register(`addresses.${index}.street`)}
        />

        <TextInput
          label="Building Number"
          id={`addresses-${index}-building`}
          placeholder="e.g. 12A or 10/2"
          error={addressErrors?.buildingNumber?.message}
          required
          register={register(`addresses.${index}.buildingNumber`)}
        />

        <TextInput
          label="Flat Number"
          id={`addresses-${index}-flat`}
          placeholder="e.g. 3B or 10"
          error={addressErrors?.flatNumber?.message}
          required
          register={register(`addresses.${index}.flatNumber`)}
        />
      </div>
    </div>
  );
};

export default AddressForm;
