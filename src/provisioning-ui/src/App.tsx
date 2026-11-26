import './App.css'

import { Routes, Route } from 'react-router';
import { MsalAuthGate } from './features/login/components/MsalAuthGate';

function App() {

  return (
    <MsalAuthGate>
      <Routes>
        <Route path="/" element={<><div>Home Page</div></>} />
        <Route path="/about" element={<><div>About Page</div></>} />
      </Routes>
    </MsalAuthGate>
  )
}

export default App
