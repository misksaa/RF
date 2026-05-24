import React, { useState, useEffect } from 'react';
import { useForm, useFieldArray } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import type { LookupDto, RegistrationDto, AddressDto } from './types';
import { apiService } from './services/api';
import TextInput from './components/TextInput';
import DateInput from './components/DateInput';
import AddressForm from './components/AddressForm';
import { PlusCircle, Save, CheckCircle, AlertTriangle, RefreshCw, UserPlus } from 'lucide-react';
import './App.css';

// 1. Validation Schemas matching Backend Rules
const nameRegex = /^[a-zA-Z\u0621-\u064A\s'\-]+$/;
const mobileRegex = /^\+[1-9]\d{6,14}$/;

export const addressSchema = z.object({
  governorateId: z.number().positive("Governorate is required."),
  cityId: z.number().positive("City is required."),
  street: z.string().trim().min(1, "Street is required.").max(200, "Street must not exceed 200 characters."),
  buildingNumber: z.string().trim().min(1, "Building number is required.").max(20, "Building number must not exceed 20 characters."),
  flatNumber: z.string().trim().min(1, "Flat number is required.").max(20, "Flat number must not exceed 20 characters."),
  isPrimary: z.boolean(),
});

export const registrationSchema = z.object({
  firstName: z.string().trim().min(1, "First name is required.").max(50, "First name must not exceed 50 characters.").regex(nameRegex, "First name must only contain Arabic or English letters, spaces, hyphens, and apostrophes."),
  middleName: z.string().trim().max(50, "Middle name must not exceed 50 characters.").regex(nameRegex, "Middle name must only contain Arabic or English letters, spaces, hyphens, and apostrophes.").optional().or(z.literal('')),
  lastName: z.string().trim().min(1, "Last name is required.").max(50, "Last name must not exceed 50 characters.").regex(nameRegex, "Last name must only contain Arabic or English letters, spaces, hyphens, and apostrophes."),
  birthDate: z.string().min(1, "Birth date is required.")
    .refine((val) => {
      const birthDate = new Date(val);
      const today = new Date();
      if (birthDate > today) return false;
      let age = today.getFullYear() - birthDate.getFullYear();
      const monthDiff = today.getMonth() - birthDate.getMonth();
      if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
        age--;
      }
      return age >= 20;
    }, "Minimum age is 20 years old at submission date."),
  mobileNumber: z.string().trim().min(1, "Mobile number is required.").regex(mobileRegex, "Mobile number must be in E.164 format (e.g. +201006158123)."),
  email: z.string().trim().min(1, "Email is required.").email("Email must be in a valid format.").max(254, "Email must not exceed 254 characters."),
  addresses: z.array(addressSchema)
    .min(1, "At least one address is required.")
    .max(5, "Maximum of 5 addresses are allowed.")
    .refine((addresses) => {
      const primaryCount = addresses.filter(a => a.isPrimary).length;
      return primaryCount === 1;
    }, "Exactly one address must be marked as primary. Click 'Set Primary' on one of your addresses."),
});

type RegistrationFormValues = z.infer<typeof registrationSchema>;

export const App: React.FC = () => {
  // State variables for lookups
  const [governorates, setGovernorates] = useState<LookupDto[]>([]);
  const [citiesByAddressIndex, setCitiesByAddressIndex] = useState<Record<number, LookupDto[]>>({});
  const [loadingCitiesByAddressIndex, setLoadingCitiesByAddressIndex] = useState<Record<number, boolean>>({});
  
  // Submit state
  const [submitStatus, setSubmitStatus] = useState<'idle' | 'submitting' | 'success' | 'error'>('idle');
  const [createdId, setCreatedId] = useState<string | null>(null);
  const [globalErrorMessage, setGlobalErrorMessage] = useState<string | null>(null);

  // Initialize React Hook Form
  const {
    register,
    control,
    handleSubmit,
    setValue,
    watch,
    setError,
    reset,
    formState: { errors }
  } = useForm<RegistrationFormValues>({
    resolver: zodResolver(registrationSchema),
    mode: 'onTouched',
    defaultValues: {
      firstName: '',
      middleName: '',
      lastName: '',
      birthDate: '',
      mobileNumber: '',
      email: '',
      addresses: [
        {
          governorateId: 0,
          cityId: 0,
          street: '',
          buildingNumber: '',
          flatNumber: '',
          isPrimary: true, // Default first address is primary
        }
      ]
    }
  });

  // Manage addresses list dynamically
  const { fields, append, remove } = useFieldArray({
    control,
    name: 'addresses'
  });

  const watchedAddresses = watch('addresses');

  // Load active governorates lookup on mount
  useEffect(() => {
    const fetchGovernorates = async () => {
      try {
        const data = await apiService.getGovernorates();
        setGovernorates(data);
      } catch (err) {
        console.error("Failed to load Governorates lookup:", err);
        setGlobalErrorMessage("Failed to load Governorates lookup from the server. Please check connection.");
      }
    };
    fetchGovernorates();
  }, []);

  // Fetch Cities when Governorate selection changes for a specific address card
  const handleGovernorateChange = async (index: number, governorateId: number) => {
    // Clear city value first since governorate has changed
    setValue(`addresses.${index}.cityId`, 0);
    
    if (!governorateId) {
      setCitiesByAddressIndex(prev => {
        const updated = { ...prev };
        delete updated[index];
        return updated;
      });
      return;
    }

    setLoadingCitiesByAddressIndex(prev => ({ ...prev, [index]: true }));
    try {
      const data = await apiService.getCities(governorateId);
      setCitiesByAddressIndex(prev => ({ ...prev, [index]: data }));
    } catch (err) {
      console.error(`Failed to load Cities lookup for governorate ${governorateId}:`, err);
    } finally {
      setLoadingCitiesByAddressIndex(prev => ({ ...prev, [index]: false }));
    }
  };

  // Add a new address item (max 5)
  const handleAddAddress = () => {
    if (fields.length >= 5) return;
    
    // Check if there is already a primary address
    const hasPrimary = watchedAddresses.some(a => a.isPrimary);

    append({
      governorateId: 0,
      cityId: 0,
      street: '',
      buildingNumber: '',
      flatNumber: '',
      isPrimary: !hasPrimary // If no primary exists, make this primary
    });
  };

  // Remove an address item (min 1)
  const handleRemoveAddress = (index: number) => {
    if (fields.length <= 1) return;

    const wasPrimary = watchedAddresses[index]?.isPrimary;
    remove(index);

    // Clean up cities selection index map
    setCitiesByAddressIndex(prev => {
      const updated = { ...prev };
      // Shift keys left to match indices
      for (let i = index; i < fields.length - 1; i++) {
        updated[i] = updated[i + 1];
      }
      delete updated[fields.length - 1];
      return updated;
    });

    // If we removed the primary address, make the first remaining address primary
    if (wasPrimary) {
      setTimeout(() => {
        setValue('addresses.0.isPrimary', true);
      }, 0);
    }
  };

  // Set selected address index as primary, resetting others
  const handleMakePrimary = (index: number) => {
    watchedAddresses.forEach((_, idx) => {
      setValue(`addresses.${idx}.isPrimary`, idx === index);
    });
  };

  // Handle Form Submit
  const onSubmit = async (values: RegistrationFormValues) => {
    setSubmitStatus('submitting');
    setGlobalErrorMessage(null);

    // Clean up and construct final DTO
    const registrationDto: Omit<RegistrationDto, 'id'> = {
      firstName: values.firstName,
      middleName: values.middleName || undefined,
      lastName: values.lastName,
      birthDate: values.birthDate,
      mobileNumber: values.mobileNumber,
      email: values.email,
      addresses: values.addresses.map((a: AddressDto) => ({
        governorateId: a.governorateId,
        cityId: a.cityId,
        street: a.street.trim(),
        buildingNumber: a.buildingNumber.trim(),
        flatNumber: a.flatNumber.trim(),
        isPrimary: a.isPrimary
      }))
    };

    try {
      const result = await apiService.createRegistration(registrationDto);
      setCreatedId(result.id);
      setSubmitStatus('success');
      reset();
    } catch (err: any) {
      setSubmitStatus('error');
      
      const response = err.response;
      if (response) {
        const status = response.status;
        const data = response.data;

        // 1. Handle 409 Conflict: Duplicates
        if (status === 409) {
          const detail = data?.detail || "";
          if (detail.toLowerCase().includes("email")) {
            setError('email', { message: "Email is already registered on our servers." });
            setGlobalErrorMessage("Duplicate Check Failure: Email already registered.");
          } else if (detail.toLowerCase().includes("mobile")) {
            setError('mobileNumber', { message: "Mobile number is already registered on our servers." });
            setGlobalErrorMessage("Duplicate Check Failure: Mobile number already registered.");
          } else {
            setGlobalErrorMessage(data?.detail || "Conflict error: duplicate email or mobile number.");
          }
        } 
        // 2. Handle 400 Bad Request with Problem Details field errors
        else if (status === 400 && data?.errors) {
          const apiErrors = data.errors;
          let mapped = false;

          Object.keys(apiErrors).forEach((key) => {
            // Map camelCase keys back to form fields
            const field = key.charAt(0).toLowerCase() + key.slice(1);
            
            if (field === 'email' || field === 'mobileNumber' || field === 'firstName' || field === 'lastName' || field === 'birthDate') {
              setError(field as any, { message: apiErrors[key][0] });
              mapped = true;
            }
          });

          setGlobalErrorMessage(mapped ? "Submission rejected: Please correct field errors below." : "Bad request: " + (data.detail || "Validation failed on the backend."));
        } else {
          setGlobalErrorMessage(data?.detail || "An unexpected error occurred. Please try again.");
        }
      } else {
        setGlobalErrorMessage("Network failure: Cannot reach API server. Check if API is running.");
      }
    }
  };

  return (
    <div className="app-container">
      <div className="glass-background"></div>
      
      <main className="form-card">
        <header className="form-header">
          <div className="logo-container">
            <UserPlus className="logo-icon" />
          </div>
          <h1>User Profile Registration</h1>
          <p className="subtitle">Secure Registration System • 3S Group</p>
        </header>

        {/* Global Success Overlay */}
        {submitStatus === 'success' && createdId && (
          <div className="success-overlay animate-fadeIn">
            <CheckCircle className="success-icon" size={60} />
            <h2>Registration Successful!</h2>
            <p className="success-detail">Your profile details have been saved in SQL Server database using EF Core persistence.</p>
            <div className="id-card">
              <span className="id-label">Registration ID:</span>
              <code className="id-value">{createdId}</code>
            </div>
            <button
              onClick={() => {
                setSubmitStatus('idle');
                setCreatedId(null);
              }}
              className="btn-primary"
            >
              Register Another User
            </button>
          </div>
        )}

        {/* Global Error Banner */}
        {globalErrorMessage && (
          <div className="global-error-banner animate-slideDown" role="alert">
            <AlertTriangle className="error-banner-icon" size={20} />
            <div className="error-banner-content">
              <h4>Submission Blocked</h4>
              <p>{globalErrorMessage}</p>
            </div>
          </div>
        )}

        <form onSubmit={handleSubmit(onSubmit)} className="registration-form" noValidate>
          {/* Section 1: Personal Profile details */}
          <section className="form-section">
            <h2 className="section-title">Personal Details</h2>
            <div className="form-grid">
              <TextInput
                label="First Name"
                id="firstName"
                placeholder="Mohamed"
                error={errors.firstName?.message}
                required
                register={register('firstName')}
              />

              <TextInput
                label="Middle Name"
                id="middleName"
                placeholder="Ahmed (Optional)"
                error={errors.middleName?.message}
                register={register('middleName')}
              />

              <TextInput
                label="Last Name"
                id="lastName"
                placeholder="Ali"
                error={errors.lastName?.message}
                required
                register={register('lastName')}
              />

              <DateInput
                label="Birth Date"
                id="birthDate"
                error={errors.birthDate?.message}
                required
                register={register('birthDate')}
              />

              <TextInput
                label="Mobile Number (E.164)"
                id="mobileNumber"
                placeholder="+201006158123"
                error={errors.mobileNumber?.message}
                required
                register={register('mobileNumber')}
              />

              <TextInput
                label="Email Address"
                id="email"
                type="email"
                placeholder="mohamed.ali@example.com"
                error={errors.email?.message}
                required
                register={register('email')}
              />
            </div>
          </section>

          {/* Section 2: Addresses List */}
          <section className="form-section">
            <div className="section-header">
              <h2 className="section-title">Addresses List</h2>
              <button
                type="button"
                className="btn-secondary btn-icon-text"
                onClick={handleAddAddress}
                disabled={fields.length >= 5}
                title="Add address (maximum of 5)"
              >
                <PlusCircle size={16} /> Add Address ({fields.length}/5)
              </button>
            </div>

            {errors.addresses?.root?.message && (
              <div className="address-validation-error">
                <AlertTriangle size={14} />
                <span>{errors.addresses.root.message}</span>
              </div>
            )}

            <div className="addresses-list">
              {fields.map((field, index) => (
                <AddressForm
                  key={field.id}
                  index={index}
                  register={register}
                  errors={errors}
                  governorates={governorates}
                  cities={citiesByAddressIndex[index] || []}
                  loadingCities={loadingCitiesByAddressIndex[index] || false}
                  canRemove={fields.length > 1}
                  onRemove={() => handleRemoveAddress(index)}
                  onGovernorateChange={(govId) => handleGovernorateChange(index, govId)}
                  onMakePrimary={() => handleMakePrimary(index)}
                  isPrimary={watchedAddresses[index]?.isPrimary || false}
                />
              ))}
            </div>
          </section>

          {/* Submit Button Controls */}
          <footer className="form-footer-actions">
            <button
              type="submit"
              className="btn-primary btn-submit-registration"
              disabled={submitStatus === 'submitting'}
            >
              {submitStatus === 'submitting' ? (
                <>
                  <RefreshCw className="animate-spin" size={18} /> Saving Profile...
                </>
              ) : (
                <>
                  <Save size={18} /> Submit Registration
                </>
              )}
            </button>
          </footer>
        </form>
      </main>
    </div>
  );
};

export default App;
