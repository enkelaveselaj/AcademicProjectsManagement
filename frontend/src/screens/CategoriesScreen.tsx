import { useCallback, useEffect, useState } from "react";
import { useAuth } from "../lib/useAuth";
import { getCategories, type Category } from "../lib/categoriesApi";
import { CategoryModal } from "./CategoryModal";

const PALETTE = [
  { bg: "bg-slate-900", text: "text-slate-900", bar: "bg-slate-900" },
  { bg: "bg-red-800", text: "text-red-800", bar: "bg-red-800" },
  { bg: "bg-emerald-700", text: "text-emerald-700", bar: "bg-emerald-700" },
  { bg: "bg-amber-700", text: "text-amber-700", bar: "bg-amber-700" },
  { bg: "bg-purple-700", text: "text-purple-700", bar: "bg-purple-700" },
  { bg: "bg-teal-700", text: "text-teal-700", bar: "bg-teal-700" },
];

function paletteFor(id: string) {
  let hash = 0;
  for (let i = 0; i < id.length; i++) {
    hash = (hash * 31 + id.charCodeAt(i)) >>> 0;
  }
  return PALETTE[hash % PALETTE.length];
}

type ModalState = "closed" | "create" | Category;

export function CategoriesScreen() {
  const { token } = useAuth();
  const [categories, setCategories] = useState<Category[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [modalState, setModalState] = useState<ModalState>("closed");

  const loadCategories = useCallback(async () => {
    if (!token) {
      return;
    }

    setIsLoading(true);
    setLoadError(null);

    try {
      setCategories(await getCategories(token));
    } catch {
      setLoadError("Could not load categories. Please try again.");
    } finally {
      setIsLoading(false);
    }
  }, [token]);

  useEffect(() => {
    void loadCategories();
  }, [loadCategories]);

  function handleSaved() {
    setModalState("closed");
    void loadCategories();
  }

  const totalProjects = categories.reduce((sum, category) => sum + category.projectCount, 0);
  const maxCount = Math.max(1, ...categories.map((category) => category.projectCount));

  return (
    <div>
      <div className="flex items-start justify-between gap-4">
        <div>
          <h1 className="font-serif text-3xl font-bold text-slate-900">Categories</h1>
          <p className="mt-1 text-sm text-slate-500">
            {categories.length} categories · {totalProjects} projects
          </p>
        </div>

        <button
          type="button"
          onClick={() => setModalState("create")}
          className="shrink-0 rounded-lg bg-slate-900 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-800"
        >
          + New Category
        </button>
      </div>

      {loadError && <p className="mt-4 text-sm text-red-600">{loadError}</p>}

      {isLoading ? (
        <p className="mt-6 text-sm text-slate-500">Loading...</p>
      ) : categories.length === 0 ? (
        <p className="mt-6 text-sm text-slate-500">No categories yet. Add the first one above.</p>
      ) : (
        <div className="mt-6 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {categories.map((category) => {
            const palette = paletteFor(category.id);
            const barWidth = Math.max(4, Math.round((category.projectCount / maxCount) * 100));

            return (
              <div key={category.id} className="rounded-xl border border-slate-200 bg-white p-5">
                <div className="flex items-center justify-between">
                  <div
                    className={`flex h-10 w-10 items-center justify-center rounded-lg font-serif text-lg font-bold text-white ${palette.bg}`}
                  >
                    {category.name[0]?.toUpperCase()}
                  </div>
                  <button
                    type="button"
                    onClick={() => setModalState(category)}
                    className="text-sm font-medium text-slate-500 hover:text-slate-900"
                  >
                    Edit
                  </button>
                </div>

                <h3 className="mt-4 font-serif text-lg font-bold text-slate-900">{category.name}</h3>
                {category.description && <p className="mt-1 text-sm text-slate-500">{category.description}</p>}

                <div className="mt-4 border-t border-slate-100 pt-4">
                  <div className="flex items-center justify-between text-sm">
                    <span className="text-slate-500">Projects</span>
                    <span className={`font-serif text-xl font-bold ${palette.text}`}>{category.projectCount}</span>
                  </div>
                  <div className="mt-2 h-1.5 w-full rounded-full bg-slate-100">
                    <div className={`h-1.5 rounded-full ${palette.bar}`} style={{ width: `${barWidth}%` }} />
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      )}

      {modalState !== "closed" && (
        <CategoryModal
          initial={modalState === "create" ? undefined : modalState}
          onClose={() => setModalState("closed")}
          onSaved={handleSaved}
        />
      )}
    </div>
  );
}
