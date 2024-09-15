import { customRef } from 'vue';

export function localRef<T = undefined>(chave: string, valor?: T) {
  return customRef<T>((track, trigger) => ({
    get: () => {
      track();
      const val = localStorage.getItem(chave);
      return val ? JSON.parse(val) : valor;
    },
    set: (val) => {
      val ? localStorage.setItem(chave, JSON.stringify(val)) : localStorage.removeItem(chave);
      trigger();
    }
  }));
}

export function sessionRef<T = undefined>(chave: string, valor?: T) {
  return customRef<T>((track, trigger) => ({
    get: () => {
      track();
      const val = sessionStorage.getItem(chave);
      return val ? JSON.parse(val) : valor;
    },
    set: (val) => {
      val ? sessionStorage.setItem(chave, JSON.stringify(val)) : sessionStorage.removeItem(chave);
      trigger();
    }
  }));
}
