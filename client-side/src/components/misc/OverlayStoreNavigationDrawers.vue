<script setup lang="ts">
import { useAuthStore, useOverlayStore } from '@/stores';
import { storeToRefs } from 'pinia';

const authStore = useAuthStore();
const { ndwDefault } = storeToRefs(useOverlayStore());
</script>

<template>
    <v-navigation-drawer v-model="ndwDefault" location="right" mobile>
    <v-list-item
        title="Projects"
      />
      <v-list-item
        title="Tasks"
      />

    <template #append>
    <v-menu>
        <template #activator="{ props }">
        <v-btn
            append-icon="mdi-account-circle"
            :text="authStore.name"
            v-bind="props"
            block
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
    </v-navigation-drawer>
</template>