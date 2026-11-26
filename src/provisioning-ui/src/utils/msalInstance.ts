import { InteractionRequiredAuthError, PublicClientApplication } from "@azure/msal-browser";
import { msalConfig } from "@/utils/msalConfig";
import { apiScopes } from "@/utils/apiScopes";

// Single MSAL instance shared by the React provider and non-React modules (API clients).
export const msalInstance = new PublicClientApplication(msalConfig);

export async function getAuthToken(): Promise<string | null> {
    const account = msalInstance.getActiveAccount() ?? msalInstance.getAllAccounts()[0] ?? null;
    if (account === null) {
        return null;
    }

    try {
        const response = await msalInstance.acquireTokenSilent({ account, scopes: apiScopes });
        return response.accessToken;
    } catch (error) {
        if (error instanceof InteractionRequiredAuthError) {
            const response = await msalInstance.acquireTokenPopup({ account, scopes: apiScopes });
            return response.accessToken;
        }
        throw error;
    }
}
