import type { EventDetails, ProblemDetails, ReserveResponse } from "./types/types";

const BASE_URL = "http://localhost:5278";

export class ApiError extends Error {
  constructor(public status: number, public problem?: ProblemDetails) {
    super(problem?.detail ?? problem?.title ?? `Request failed with ${status}`);
  }
}

async function handleResponse<T>(res: Response): Promise<T> {
  if (res.ok) {
    if (res.status === 204) return undefined as T;
    return res.json();
  }
  const problem = await res.json().catch(() => undefined);
  throw new ApiError(res.status, problem);
}

export function getEvent(eventId: number): Promise<EventDetails> {
  return fetch(`${BASE_URL}/api/events/${eventId}`).then(handleResponse<EventDetails>);
}

export function reserveTicket(eventId: number, holderName: string): Promise<ReserveResponse> {
  return fetch(`${BASE_URL}/api/events/${eventId}/reserve`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ holderName }),
  }).then(handleResponse<ReserveResponse>);
}

export function purchaseTicket(ticketId: number, holderName: string): Promise<void> {
  return fetch(`${BASE_URL}/api/tickets/${ticketId}/purchase`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ holderName }),
  }).then(handleResponse<void>);
}