interface ErrorMessageProps {
  message: string;
  onRetry?: () => void;
}

export default function ErrorMessage({ message, onRetry }: ErrorMessageProps) {
  return (
    <div className="p-4 max-w-md mx-auto">
      <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded relative">
        <strong className="font-bold">Error!</strong>
        <span className="block sm:inline ml-2">{message}</span>
        {onRetry && (
          <button
            onClick={onRetry}
            className="absolute top-0 right-0 px-4 py-3 text-red-700 hover:text-red-500"
          >
            Retry
          </button>
        )}
      </div>
    </div>
  );
}