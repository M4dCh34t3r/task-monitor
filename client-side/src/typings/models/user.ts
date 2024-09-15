export interface User {
  id: string;
  userName: string;
  password: string;
  createdAt: string | number;
  updatedAt?: string | number;
  deletedAt?: string | number;
}
