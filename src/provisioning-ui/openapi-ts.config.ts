
import { defineConfig } from '@hey-api/openapi-ts';

export default defineConfig([
{
  input: '../Provisioning/SmartInventory.Provisioning.API/openapi/SmartInventory.Provisioning.API.json',
  output: {
    path: 'src/generated/api',
  },
}  
]);
