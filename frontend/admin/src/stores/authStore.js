import { defineStore } from "pinia";

export const useAuthStore = defineStore("auth", {
    state: () => ({
        admin: null,
    }),

    actions: {
        setAdmin(admin) {
            this.admin = admin;
        },

        logout() {
            this.admin = null;
        },
    },
});
