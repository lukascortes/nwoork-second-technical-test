// Extracts a human-friendly message from an Axios error whose body follows the
// RFC 7807 ProblemDetails / ValidationProblemDetails shape returned by the API.
export function getApiErrorMessage(error: unknown, fallback = 'Something went wrong'): string {
  const data = (error as { response?: { data?: unknown } })?.response?.data;

  if (typeof data === 'string') return data;

  if (data && typeof data === 'object') {
    const problem = data as {
      detail?: string;
      title?: string;
      errors?: Record<string, string[]>;
    };

    if (problem.errors) {
      const firstField = Object.values(problem.errors)[0];
      if (Array.isArray(firstField) && firstField.length > 0) {
        return firstField[0];
      }
    }
    if (problem.detail) return problem.detail;
    if (problem.title) return problem.title;
  }

  return error instanceof Error ? error.message : fallback;
}
