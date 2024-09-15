<script setup lang="ts">
import { useAuthStore, useConfigStore, useOverlayStore } from '@/stores';
import { storeToRefs } from 'pinia';
import { computed } from 'vue';

const authStore = useAuthStore();
const { inDarkMode } = storeToRefs(useConfigStore());
const { ndwDefault } = storeToRefs(useOverlayStore());

const icnThemeIcon = computed(() => (inDarkMode.value ? 'mdi-weather-sunny' : 'mdi-weather-night'));
</script>

<template>
  <v-app-bar v-if="authStore.id" align="center" rounded="0">
    <template #prepend>
      <v-btn icon="mdi-home" height="36" width="36" rounded to="/" />
    </template>
    <template #append>
      <v-scroll-y-transition mode="out-in">
        <v-icon
          @click="() => (inDarkMode = !inDarkMode)"
          :icon="icnThemeIcon"
          :key="icnThemeIcon"
          variant="text"
          size="x-large"
          class="mr-2"
        />
      </v-scroll-y-transition>
      <v-icon @click="() => ndwDefault = true" class="mr-2">
        {{ 'mdi-menu' }}
      </v-icon>
    </template>
  </v-app-bar>
</template>
