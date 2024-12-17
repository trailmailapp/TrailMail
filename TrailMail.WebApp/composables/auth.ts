import { useQuery, useMutation } from "@tanstack/vue-query";
import { $api } from './api';

export function useWhoAmI() {
    return useQuery({
        queryKey: ['whoAmI'],
        queryFn: () => $api('/Auth/WhoAmI'),
        retry: false,
    });
}

export function useLoginMutation() {
    return useMutation({
        mutationFn: () => {
            return Promise.resolve(window.location.href = '/Auth/Login');
        }
    });
}