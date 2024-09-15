import { assertType } from '@/helpers/typeAssertHelper';
import router from '@/plugins/router';
import type { JwtClaimsDTO } from '@/typings';
import { jwtParse } from '@/utils/jwtUtil';
import { localRef } from '@/utils/refUtil';
import { defineStore } from 'pinia';

export const useAuthStore = defineStore('auth', {
  state: () => ({
    expiresAt: localRef<number | undefined>('expires-at'),
    admin: localRef<boolean | undefined>('admin'),
    token: localRef<string | undefined>('token'),
    name: localRef<string | undefined>('name'),
    id: localRef<string | undefined>('id')
  }),
  actions: {
    login(token: string, redirect: boolean = true) {
      this.updateToken(token);
      if (redirect) router.push('/');
    },
    logout(redirect: boolean = true) {
      this.admin = undefined;
      this.token = undefined;
      this.name = undefined;
      this.id = undefined;

      if (redirect) router.push('/login');
    },
    removeToken() {
      this.expiresAt = undefined;
      this.token = undefined;
    },
    updateToken(token: string) {
      if (this.token === token) return;

      const jwtClaims = assertType<JwtClaimsDTO>({ jwtClaimsDTO: jwtParse(token) });
      this.expiresAt = jwtClaims.exp;
      this.admin = jwtClaims.adm;
      this.name = jwtClaims.unm;
      this.id = jwtClaims.uid;
      this.token = token;
    }
  }
});
