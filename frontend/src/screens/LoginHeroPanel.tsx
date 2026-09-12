const STATS = [
  { value: "48", label: "Active Projects" },
  { value: "12", label: "Departments" },
  { value: "240+", label: "Students" },
] as const;

export function LoginHeroPanel() {
  return (
    <div className="relative hidden w-1/2 flex-col justify-between overflow-hidden bg-slate-900 px-14 py-16 text-white lg:flex">
      <div className="pointer-events-none absolute -top-24 -right-24 h-72 w-72 rounded-full bg-white/5" />

      <div className="flex items-center gap-4">
        <div className="flex h-14 w-14 items-center justify-center rounded-lg bg-red-800 font-serif text-2xl font-bold">
          A
        </div>
        <div>
          <p className="font-serif text-2xl font-bold leading-tight">Academic Projects</p>
          <p className="text-sm text-slate-400">University Management Platform</p>
        </div>
      </div>

      <div className="max-w-lg">
        <h2 className="font-serif text-5xl leading-tight font-bold">
          Manage academic <em className="text-red-400 italic">projects</em> with confidence.
        </h2>
        <p className="mt-5 text-base text-slate-300">
          A unified platform for students, mentors, and administrators to collaborate, track progress, and deliver
          exceptional academic research.
        </p>
      </div>

      <div>
        <div className="flex gap-12 border-t border-slate-700 pt-7">
          {STATS.map((stat) => (
            <div key={stat.label}>
              <p className="font-serif text-4xl font-bold">{stat.value}</p>
              <p className="mt-1 text-sm text-slate-400">{stat.label}</p>
            </div>
          ))}
        </div>
        <p className="mt-6 text-sm text-slate-500">© 2026 University Academic Affairs. All rights reserved.</p>
      </div>
    </div>
  );
}
