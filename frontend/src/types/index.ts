export interface LookupDto {
  id: number;
  nameAr: string;
  nameEn: string;
}

export interface AddressDto {
  id?: string;
  governorateId: number;
  cityId: number;
  street: string;
  buildingNumber: string;
  flatNumber: string;
  isPrimary: boolean;
  governorateNameAr?: string;
  governorateNameEn?: string;
  cityNameAr?: string;
  cityNameEn?: string;
}

export interface RegistrationDto {
  id?: string;
  firstName: string;
  middleName?: string;
  lastName: string;
  birthDate: string;
  mobileNumber: string;
  email: string;
  addresses: AddressDto[];
}
