// src/App.tsx
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import LoginPage from './pages/Login/LoginPage';
import AdminDashboard from './pages/AdminDashboard/AdminDashboard';
import EmployeeDashboard from './pages/EmployeeDashboard/EmployeeDashboard';
import ProtectedRoute from './components/common/ProtectedRoute';
import UnauthorizedPage from './pages/UnauthorizedPage';
import UsersPage from './pages/admin/UsersPage';

function App() {
  return (
    <Router>
      <Routes>
      
        <Route path="/login" element={<LoginPage />} />
        
        
        <Route element={<ProtectedRoute allowedRoles={['Admin']} />}>
          <Route path="/dashboard/admin" element={<AdminDashboard />} />
          <Route path="/dashboard/admin/users" element={<UsersPage />} />
        </Route>
        
       
        <Route element={<ProtectedRoute allowedRoles={['Employee']} />}>
          <Route path="/dashboard/employee" element={<EmployeeDashboard />} />
        </Route>
        
       
        <Route path="/unauthorized" element={<UnauthorizedPage />} />
        
        
        <Route path="/" element={<Navigate to="/login" replace />} />
        <Route path="*" element={<Navigate to="/login" replace />} />
      </Routes>
    </Router>
  );
}

export default App;