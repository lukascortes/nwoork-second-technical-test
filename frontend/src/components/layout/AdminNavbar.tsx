import { Link } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';

export default function AdminNavbar() {
  const { logout } = useAuth();

  return (
    <nav className="bg-gradient-to-r from-purple-700 to-purple-500 shadow-lg">
      <div className="max-w-7xl mx-auto px-6 py-4">
        <div className="flex items-center justify-between">
         
          <div className="flex-shrink-0">
            <span className="text-xl font-bold text-white tracking-tight">
              Time Off Manager
            </span>
          </div>
          
         
          <div className="hidden md:flex items-center space-x-1 ml-10">
            <Link 
              to="/dashboard/admin" 
              className="px-4 py-2 rounded-md text-sm font-medium text-purple-100 hover:text-white hover:bg-purple-600 transition duration-200"
            >
              <span className="flex items-center">
                <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
                </svg>
                Requests
              </span>
            </Link>
            
            <Link 
              to="/dashboard/admin/users" 
              className="px-4 py-2 rounded-md text-sm font-medium text-purple-100 hover:text-white hover:bg-purple-600 transition duration-200"
            >
              <span className="flex items-center">
                <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
                </svg>
                Users
              </span>
            </Link>
          </div>
          
         
          <div className="flex-shrink-0">
            <button
              onClick={logout}
              className="flex items-center space-x-2 px-4 py-2 rounded-full text-sm font-medium text-white bg-purple-800 hover:bg-purple-900 transition duration-200 shadow-md"
            >
              <span>Logout</span>
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
              </svg>
            </button>
          </div>
        </div>
        
       
        <div className="md:hidden mt-3 flex justify-between">
          <Link 
            to="/dashboard/admin" 
            className="px-3 py-1 text-xs font-medium text-purple-100 hover:text-white"
          >
            Requests
          </Link>
          <Link 
            to="/dashboard/admin/users" 
            className="px-3 py-1 text-xs font-medium text-purple-100 hover:text-white"
          >
            Users
          </Link>
        </div>
      </div>
    </nav>
  );
}