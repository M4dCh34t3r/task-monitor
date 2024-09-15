<script setup lang="ts">
import { storeToRefs } from 'pinia';
import { useAuthStore, useOverlayStore } from '@/stores';
import { extractTimeLeft } from '@/utils/dateTimeUtil';
import { jwtParse } from '@/utils/jwtUtil';
import { computed, onMounted, onUnmounted, ref } from 'vue';

let intervalId = -1;
let timeoutId = -1;

const authStore = useAuthStore();
const overlayStore = useOverlayStore();
const { snbSessionExpired, snbTimeLeft } = storeToRefs(overlayStore);

const snbTimeLeftText = ref<string>('');

const jwtExpirationTime = computed(() => Number(authStore.expiresAt) * 1000);

function addEventListeners() {
  document.addEventListener('mousemove', timerReset);
  document.addEventListener('keypress', timerReset);
  document.addEventListener('click', timerReset);
}

function clearCountdowns() {
  clearInterval(intervalId);
  clearTimeout(timeoutId);
}

function removeEventListeners() {
  document.removeEventListener('mousemove', timerReset);
  document.removeEventListener('keypress', timerReset);
  document.removeEventListener('click', timerReset);
}

function snbTimeLeftInterval() {
  if (!authStore.id || !authStore.expiresAt) return;

  const now = Date.now();
  if (jwtExpirationTime.value > now) {
    const [minutes, seconds] = extractTimeLeft(jwtExpirationTime.value, now);
    const minutesStr = seconds.toString().padStart(2, '0');
    const secondsStr = seconds.toString().padStart(2, '0');
    const snbTimeLeftTextSuffix =
      minutes === 0 ? `${secondsStr} sec.` : `${minutesStr} min. and ${secondsStr} sec.`;
    snbTimeLeftText.value = `Your session will expire in ${snbTimeLeftTextSuffix}`;
    snbTimeLeft.value = true;
    return;
  }

  if (overlayStore.dlgSessionExpired) {
    removeEventListeners();
    clearCountdowns();
    return;
  }

  snbSessionExpired.value = true;
  timerReset();
}

function timerReset() {
  clearCountdowns();

  snbTimeLeft.value = false;

  if (snbSessionExpired.value) return;

  timeoutId = setTimeout(() => {
    intervalId = setInterval(snbTimeLeftInterval, 1000);
    snbTimeLeftInterval();
  }, 5000);
}

onMounted(() => {
  const authStore = useAuthStore();
  if (authStore.token && jwtParse(authStore.token).exp < Math.floor(Date.now() / 1000)) {
    snbSessionExpired.value = true;
    authStore.logout();
  } else {
    timerReset();
    addEventListeners();
  }
});

onUnmounted(() => {
  removeEventListeners();
  clearCountdowns();
});
</script>

<template>
  <v-snackbar v-model="snbTimeLeft" location="top right" color="mix-inverse" :timeout="-1">
    <div class="text-center">{{ snbTimeLeftText }}</div>
  </v-snackbar>

  <v-snackbar v-model="snbSessionExpired" location="bottom right" color="warning" timeout="5000">
    <div class="text-center">The session has expired</div>
  </v-snackbar>
</template>
