export enum UserRole {
  Patient = 1,
  Doctor = 2,
  Admin = 3
}

export enum AppointmentStatus {
  Scheduled = 1,
  Completed = 2,
  Cancelled = 3
}

export interface User {
  id: number;
  email: string;
  name: string;
  role: UserRole;
  createdAt: string;
}

export interface Doctor {
  id: number;
  userId: number;
  specialty: string;
  phone: string;
  user: User;
}

export interface Appointment {
  id: number;
  patientId: number;
  doctorId: number;
  date: string;
  time: string;
  status: AppointmentStatus;
  reason: string;
  createdAt: string;
  patient: User;
  doctor: Doctor;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  name: string;
  role: UserRole;
}

export interface CreateAppointmentRequest {
  patientId: number;
  doctorId: number;
  date: string;
  time: string;
  reason: string;
}

export interface AuthResponse {
  token: string;
  user: User;
}
