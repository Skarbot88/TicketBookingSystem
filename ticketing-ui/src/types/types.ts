// src/types.ts
export interface EventDetails {
  id: number;
  name: string;
  startsAt: string;
  totalSeats: number;
  availableCount: number;
  reservedCount: number;
  soldCount: number;
}

export type ReserveResult =
  | { status: 201; ticketId: number }
  | { status: 400; message: string }
  | { status: 404; message: string }
  | { status: 409; message: string };

  export interface ProblemDetails {
  title: string;
  status: number;
  detail?: string;
}

export interface ReserveResponse {
  ticketId: number;
}