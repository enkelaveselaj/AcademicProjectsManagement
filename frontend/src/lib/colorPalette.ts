export interface PaletteEntry {
  bg: string;
  text: string;
  bar: string;
  soft: string;
  softText: string;
}

const PALETTE: PaletteEntry[] = [
  { bg: "bg-slate-900", text: "text-slate-900", bar: "bg-slate-900", soft: "bg-slate-100", softText: "text-slate-700" },
  { bg: "bg-red-800", text: "text-red-800", bar: "bg-red-800", soft: "bg-red-50", softText: "text-red-700" },
  {
    bg: "bg-emerald-700",
    text: "text-emerald-700",
    bar: "bg-emerald-700",
    soft: "bg-emerald-50",
    softText: "text-emerald-700",
  },
  {
    bg: "bg-amber-700",
    text: "text-amber-700",
    bar: "bg-amber-700",
    soft: "bg-amber-50",
    softText: "text-amber-800",
  },
  {
    bg: "bg-purple-700",
    text: "text-purple-700",
    bar: "bg-purple-700",
    soft: "bg-purple-50",
    softText: "text-purple-700",
  },
  { bg: "bg-teal-700", text: "text-teal-700", bar: "bg-teal-700", soft: "bg-teal-50", softText: "text-teal-700" },
];

export function paletteFor(id: string): PaletteEntry {
  let hash = 0;
  for (let i = 0; i < id.length; i++) {
    hash = (hash * 31 + id.charCodeAt(i)) >>> 0;
  }
  return PALETTE[hash % PALETTE.length];
}
