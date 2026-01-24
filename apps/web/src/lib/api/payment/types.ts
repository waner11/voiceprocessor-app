// Payment Pack Types
export interface CreditPack {
  id: string;
  name: string;
  credits: number;
  price: number;
  description: string;
}

export interface CheckoutSessionResponse {
  checkoutUrl: string;
}

export interface PaymentVerificationResponse {
  success: boolean;
  creditsAdded: number;
  newBalance: number;
}
