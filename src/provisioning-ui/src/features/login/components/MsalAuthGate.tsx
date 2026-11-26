
import { useEffect, useRef, type ReactNode } from "react";
import { useMsalAuth } from "@/features/login/hooks/useMsalAuth";
import { Button } from "@/components/ui/button";
import { AuthContextProvider } from "../hooks/useAuthContext";

interface MsalAuthGateProps {
    children: ReactNode;
}

export const MsalAuthGate = ({ children }: MsalAuthGateProps) => {
        const { account, 
        isAuthenticated, 
        isLoading, 
        login, 
        logout } = useMsalAuth();

    const autoLoginAttempted = useRef(false);

    // Attempt automatic sign-in only once; afterwards the user uses the button.
    useEffect(() => {
        if (!isAuthenticated && !isLoading && !autoLoginAttempted.current) {
            autoLoginAttempted.current = true;
            login().catch(() => { /* already logged; user can retry with the button */ });
        }
    }, [isAuthenticated, isLoading]);

    if (isAuthenticated) {
        const user = account === null ? null
            : { id: account.homeAccountId, username: account.username };
        
            console.log(`Authenticated as ${user?.username}`);

        return <AuthContextProvider value={{ user, logout }}>
            {children}
            </AuthContextProvider>;
    }

    return <>
        <div className="flex flex-col items-center justify-center min-h-screen">
            Please sign in to continue.
            <div >
                <Button className="mt-4"  onClick={login} disabled={isLoading}>
                    {isLoading ? "Signing in..." : "Login with MSAL"}
                </Button>
            </div>            
        </div>
    </>;
};
