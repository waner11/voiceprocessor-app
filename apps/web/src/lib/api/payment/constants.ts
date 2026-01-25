import { CreditPack } from "./types";

export const CREDIT_PACKS: CreditPack[] = [
  {
    id: "pack_short_story",
    name: "Short Story",
    credits: 15000,
    price: 4.99,
    priceId: "price_short_story",
    description: "Perfect for short stories or testing.",
  },
  {
    id: "pack_novella",
    name: "Novella",
    credits: 50000,
    price: 19.99,
    priceId: "price_novella",
    description: "Great for novellas or long chapters.",
  },
  {
    id: "pack_audiobook",
    name: "Audiobook",
    credits: 120000,
    price: 39.99,
    priceId: "price_audiobook",
    description: "Best value for full-length books.",
  },
];
