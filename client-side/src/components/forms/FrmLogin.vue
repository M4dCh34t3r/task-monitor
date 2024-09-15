<script setup lang="ts">
import { HttpStatusCode, type AxiosInstance } from 'axios';
import type { AuthRequestDTO } from '@/typings';
import { computed, inject, onMounted, ref } from 'vue';
import { VForm, type VCheckbox, type VIcon } from 'vuetify/components';
import { useAuthStore } from '@/stores/auth';
import { useConfigStore } from '@/stores/config';

const appVersion = import.meta.env.PACKAGE_VERSION;
const axios = inject<AxiosInstance>('axios')!;
const btnLoginLoading = ref<boolean>(false);
const ckbRememberMe = ref<boolean>(false);
const configStore = useConfigStore();
const frmLoginBusy = ref<boolean>(false);
const txfUserName = ref<string>('');
const txfPassword = ref<string>('');
const txfPasswordShow = ref<boolean>(false);

const btnLoginDisabled = computed(
  () =>
    frmLoginBusy.value ||
    txfPassword.value.length === 0 ||
    txfPasswordLengthValid(txfPassword.value) !== true ||
    !/^[A-Za-z._]+$/.test(txfUserName.value)
);
const txfClass = computed(() => (configStore.inMobileMode ? 'mx-4' : 'mx-8'));
const txfPasswordIcon = computed(() => (txfPasswordShow.value ? 'mdi-eye-off' : 'mdi-eye'));
const txfPasswordType = computed(() => (txfPasswordShow.value ? 'text' : 'password'));

async function frmLoginSubmit(e: Event) {
  const data: AuthRequestDTO = {
    userName: txfUserName.value,
    password: txfPassword.value
  };

  btnLoginLoading.value = true;
  e.preventDefault();

  try {
    const res = await axios.post('Auth/Login', data);
    if (res?.status === HttpStatusCode.Ok) {
      if (ckbRememberMe.value) {
        configStore.lastUserName = txfUserName.value;
        configStore.lastPassword = txfPassword.value;
      } else {
        configStore.lastUserName = undefined;
        configStore.lastPassword = undefined;
      }
      useAuthStore().login(res.data);
    }
  } finally {
    btnLoginLoading.value = false;
  }
}

async function preencherCampos(textoLogin: string, textoSenha: string) {
  if (txfUserName.value !== textoLogin) {
    while (txfUserName.value.length > 0) {
      await new Promise((resolve) => {
        setTimeout(() => resolve((txfUserName.value = txfUserName.value.slice(0, -1))), 50);
      });
    }

    for (let i = 0; i < textoLogin.length; i++) {
      await new Promise((resolve) => {
        setTimeout(() => resolve((txfUserName.value += textoLogin[i])), 100);
      });
    }
  }

  if (txfPassword.value !== textoSenha) {
    while (txfPassword.value.length > 0) {
      await new Promise((resolve) => {
        setTimeout(() => resolve((txfPassword.value = txfPassword.value.slice(0, -1))), 50);
      });
    }

    for (let i = 0; i < textoSenha.length; i++) {
      await new Promise((resolve) => {
        setTimeout(() => resolve((txfPassword.value += textoSenha[i])), 100);
      });
    }
  }
}

function txfUserNameOnKeyDown(e: KeyboardEvent) {
  if (frmLoginBusy.value) e.preventDefault();
  if (/^[A-Za-z._]+$/.test(e.key)) return true;
  e.preventDefault();
}

function txfPasswordOnKeyDown(e: KeyboardEvent) {
  if (frmLoginBusy.value) e.preventDefault();
}

function txfPasswordAppendInnerClick() {
  txfPasswordShow.value = !txfPasswordShow.value;
}

function txfPasswordLengthValid(val: string) {
  if (val.length === 0) return true;
  if (val.length < 4) return 'The password has at least 4 characters...';
  if (val.length > 512) return 'The password has at max 512 characters...';
  return true;
}

onMounted(async () => {
  if (configStore.lastUserName) {
    frmLoginBusy.value = true;
    await new Promise((resolve) => setTimeout(resolve, 250));
    await preencherCampos(configStore.lastUserName!, configStore.lastPassword!);
    await new Promise((resolve) => setTimeout(resolve, 250));
    ckbRememberMe.value = true;
    await new Promise((resolve) => setTimeout(resolve, 250));
    frmLoginBusy.value = false;
  }
});
</script>

<template>
  <v-form @submit="frmLoginSubmit">
    <v-card elevation="8" width="100%">
      <v-card-title class="text-center bg-primary py-4">WELCOME!</v-card-title>
      <v-text-field
        :onkeydown="txfUserNameOnKeyDown"
        prepend-icon="mdi-account-circle"
        v-model="txfUserName"
        variant="outlined"
        :class="txfClass"
        label="User name"
        class="my-6"
      />
      <v-text-field
        :onkeydown="txfPasswordOnKeyDown"
        :rules="[txfPasswordLengthValid]"
        :type="txfPasswordType"
        prepend-icon="mdi-key"
        :hide-details="false"
        v-model="txfPassword"
        variant="outlined"
        :class="txfClass"
        label="Password"
      >
        <template #append-inner>
          <v-fade-transition leave-absolute>
            <v-icon @click="txfPasswordAppendInnerClick" :key="txfPasswordIcon">
              {{ txfPasswordIcon }}
            </v-icon>
          </v-fade-transition>
        </template>
      </v-text-field>

      <v-fade-transition>
        <v-checkbox v-model="ckbRememberMe" label="Remember me" class="mx-8" />
      </v-fade-transition>
      <v-btn
        max-width="min(calc(100% - 32px), 480px)"
        :disabled="btnLoginDisabled"
        :loading="btnLoginLoading"
        class="mx-auto my-4"
        text="login"
        type="submit"
        color="info"
        width="100%"
      />
      <v-row class="text-medium-emphasis" justify="space-between" align="end">
        <v-col class="ml-2 mb-1" align="start">Version {{ appVersion }}</v-col>
        <v-col class="mr-2 mb-1" align="end">© 2024<br />Pedro Andrade</v-col>
      </v-row>
      <v-switch
        class="position-absolute right-0 top-0 mx-4 my-2"
        v-model="configStore.inDarkMode"
        false-icon="mdi-weather-sunny"
        true-icon="mdi-weather-night"
      />
    </v-card>
  </v-form>
</template>

<style scoped>
.animate {
  animation: animation 1s ease-out;
}

@keyframes animation {
  0% {
    opacity: 0;
  }
  100% {
    opacity: 1;
  }
}
</style>
