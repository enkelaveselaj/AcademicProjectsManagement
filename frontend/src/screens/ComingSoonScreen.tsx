interface ComingSoonScreenProps {
  title: string;
}

export function ComingSoonScreen({ title }: ComingSoonScreenProps) {
  return (
    <div>
      <h1 className="font-serif text-3xl font-bold text-slate-900">{title}</h1>
      <p className="mt-2 text-sm text-slate-500">This screen is coming in a future update.</p>
    </div>
  );
}
