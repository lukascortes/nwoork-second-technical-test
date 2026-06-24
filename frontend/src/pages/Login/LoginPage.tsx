import { useLogin } from './useLogin';
import logo from '../../img/nwoork.png';

export default function LoginPage() {
  const {
    email,
    setEmail,
    password,
    setPassword,
    error,
    loading,
    handleSubmit,
  } = useLogin();

  return (
    <section className="min-h-screen bg-gray-50 flex items-center justify-center p-4">
      <div className="w-full max-w-4xl bg-white rounded-xl shadow-md overflow-hidden">
        <div className="flex flex-col lg:flex-row">
         
          <div className="w-full lg:w-1/2 p-8 md:p-12">
          
            <div className="text-center mb-8">
              <img
                className="mx-auto"
                src={logo}
                alt="logo"
              />
              <h4 className="mt-4 text-2xl font-semibold text-gray-800">
                Welcome back, please log in
              </h4>
            </div>

            <form onSubmit={handleSubmit}>
             
              <div className="mb-6">
                <label htmlFor="username" className="block text-sm font-medium text-gray-700 mb-2">
                  Email
                </label>
                <input
                  type="text"
                  id="username"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                  placeholder="Enter your email"
                  required
                />
              </div>

             
              <div className="mb-6">
                <label htmlFor="password" className="block text-sm font-medium text-gray-700 mb-2">
                  Password
                </label>
                <input
                  type="password"
                  id="password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                  placeholder="Enter your password"
                  required
                />
              </div>

             
              <div className="flex items-center justify-between mb-8">
                <div className="flex items-center">
                  <input
                    id="remember-me"
                    name="remember-me"
                    type="checkbox"
                    className="h-4 w-4 text-purple-600 focus:ring-purple-500 border-gray-300 rounded"
                  />
                  <label htmlFor="remember-me" className="ml-2 block text-sm text-gray-700">
                    Remember me
                  </label>
                </div>
                <a href="#!" className="text-sm text-purple-600 hover:text-purple-800">
                  Forgot your password?
                </a>
              </div>

           
              {error && (
                <div className="mb-4 p-3 bg-red-100 text-red-700 rounded-md text-sm">{error}</div>
              )}

              <button
                type="submit"
                disabled={loading}
                className="w-full py-3 px-4 bg-gradient-to-r from-purple-700 to-purple-500 hover:from-purple-800 hover:to-purple-600 text-white font-medium rounded-lg shadow-md transition duration-300 disabled:opacity-60"
              >
                {loading ? 'Signing in...' : 'Log in'}
              </button>

            
            </form>
          </div>

          
          <div className="w-full lg:w-1/2 bg-gradient-to-br from-purple-700 to-purple-500 p-8 md:p-12 flex items-center">
            <div className="text-white">
              <h2 className="text-3xl font-bold mb-4">Time off manager system</h2>
              <p className="mb-6">
                  Centralized platform for managing time-off requests.
              </p>
              <ul className="space-y-3">
                <li className="flex items-start">
                  <svg className="h-5 w-5 text-purple-200 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                  </svg>
                  Request and manage your days off (vacation, sick, personal)
                </li>
                <li className="flex items-start">
                  <svg className="h-5 w-5 text-purple-200 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                  </svg>
                  View the status of all your requests in real time.
                </li>
                <li className="flex items-start">
                  <svg className="h-5 w-5 text-purple-200 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                  </svg>
                  Secure access with differentiated roles.
                </li>
                <li className="flex items-start">
                  <svg className="h-5 w-5 text-purple-200 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                  </svg>
                  Easy to use and intuitive interface.
                </li>
              </ul>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}