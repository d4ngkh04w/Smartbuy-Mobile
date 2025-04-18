import { createPinia } from "pinia";

// Create Pinia instance
const pinia = createPinia();

// Export for use in components
export default pinia;

// Export a function to get the store outside of Vue components
export function getStoreInstance(storeId, store) {
    // This allows access to the store outside of the Vue component lifecycle
    return store(pinia);
}
