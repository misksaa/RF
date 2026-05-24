import { describe, it, expect } from 'vitest';
import { registrationSchema } from './App';

describe('Registration Form Validation Schema', () => {
  const validData = {
    firstName: "Mohamed",
    middleName: "Ahmed",
    lastName: "Ali",
    birthDate: "2000-05-24",
    mobileNumber: "+201006158123",
    email: "mohamed.ali@example.com",
    addresses: [
      {
        governorateId: 1,
        cityId: 1,
        street: "Tahrir St",
        buildingNumber: "12",
        flatNumber: "3A",
        isPrimary: true
      }
    ]
  };

  it('should accept valid registration values', () => {
    const result = registrationSchema.safeParse(validData);
    expect(result.success).toBe(true);
  });

  it('should reject names containing digits', () => {
    const invalidName = { ...validData, firstName: "Mohamed123" };
    const result = registrationSchema.safeParse(invalidName);
    expect(result.success).toBe(false);
    if (!result.success) {
      expect(result.error.issues[0].message).toContain("must only contain Arabic or English letters");
    }
  });

  it('should support Arabic names', () => {
    const arabicName = { ...validData, firstName: "محمد", lastName: "علي" };
    const result = registrationSchema.safeParse(arabicName);
    expect(result.success).toBe(true);
  });

  it('should reject birth dates under 20 years old', () => {
    const today = new Date();
    const underAgeDate = `${today.getFullYear() - 19}-${String(today.getMonth() + 1).padStart(2, '0')}-${String(today.getDate()).padStart(2, '0')}`;
    
    const youngData = { ...validData, birthDate: underAgeDate };
    const result = registrationSchema.safeParse(youngData);
    expect(result.success).toBe(false);
    if (!result.success) {
      expect(result.error.issues[0].message).toContain("Minimum age is 20 years old");
    }
  });

  it('should reject mobile numbers not in E.164 format', () => {
    const invalidPhone = { ...validData, mobileNumber: "01006158123" };
    const result = registrationSchema.safeParse(invalidPhone);
    expect(result.success).toBe(false);
  });

  it('should reject empty addresses list', () => {
    const noAddresses = { ...validData, addresses: [] };
    const result = registrationSchema.safeParse(noAddresses);
    expect(result.success).toBe(false);
  });

  it('should reject multiple primary addresses', () => {
    const multiplePrimaries = {
      ...validData,
      addresses: [
        {
          governorateId: 1,
          cityId: 1,
          street: "Tahrir St",
          buildingNumber: "12",
          flatNumber: "3A",
          isPrimary: true
        },
        {
          governorateId: 1,
          cityId: 1,
          street: "Mohamed St",
          buildingNumber: "15",
          flatNumber: "2",
          isPrimary: true
        }
      ]
    };
    const result = registrationSchema.safeParse(multiplePrimaries);
    expect(result.success).toBe(false);
  });
});
