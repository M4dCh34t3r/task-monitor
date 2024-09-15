<script setup lang="ts">
import { useAuthStore, useConfigStore } from '@/stores';
import { storeToRefs } from 'pinia';
import { computed } from 'vue';

const authStore = useAuthStore();
const { inDarkMode } = storeToRefs(useConfigStore());

const icnThemeIcon = computed(() => (inDarkMode.value ? 'mdi-weather-sunny' : 'mdi-weather-night'));
</script>

<template>
  <v-app-bar v-if="authStore.id" align="center" rounded="0">
    <template #prepend>
      <v-btn icon="mdi-home" height="36" width="36" rounded to="/" />
      <v-btn append-icon="mdi-file-sign" text="Projects" width="8.75rem" />
      <v-btn append-icon="mdi-calendar" width="8.75rem" text="Tasks" />
    </template>
    <img src="/title.svg" height="32px" />
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
      <v-menu>
        <template #activator="{ props }">
          <v-btn
            append-icon="mdi-account-circle"
            :text="authStore.name"
            min-width="128"
            v-bind="props"
          />
        </template>
        <v-list>
          <v-list-item title="Update account info" disabled />
          <v-list-item title="Change password" disabled />
          <v-divider />
          <v-list-item @click="authStore.logout()" append-icon="mdi-logout" title="Exit" />
        </v-list>
      </v-menu>
    </template>
  </v-app-bar>
</template>
