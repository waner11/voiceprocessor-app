// Payment Pack Types
export interface CreditPack {
  id: string;
  name: string;
  credits: number;
  price: number;
  priceId: string;
  description: string;
}

export interface CheckoutRequest {
  priceId: string;
}

export interface CheckoutSessionResponse {
  checkoutUrl: string;
}

export interface PaymentVerificationResponse {
  success: boolean;
  creditsAdded: number;
  newBalance: number;
}

// Payment History Types
export interface Payment {
  id: string;
  packName: string;
  creditsAdded: number;
  amountPaid: number;
  currency: string;
  status: 'completed' | 'pending' | 'failed' | 'refunded';
  createdAt: string;
}

export interface PaymentHistoryResponse {
  payments: Payment[];
}
