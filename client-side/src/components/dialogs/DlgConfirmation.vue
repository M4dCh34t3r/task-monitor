<script setup lang="ts">
import TbrWindow from '../toolbars/TbrWindow.vue';

const model = defineModel<boolean>({ default: false });

defineEmits<{
  (e: 'btnYesClick'): void;
}>();

defineProps<{
  loading: boolean;
  icon: string;
  text: string;
  title: string;
}>();
</script>

<template>
  <v-dialog :persistent="true" v-model="model" width="480">
    <v-card :loading="loading">
      <TbrWindow :btn-fechar-disabled="loading" @click="() => (model = false)" :title="title" />
      <v-card-text class="text-center">
        {{ text }}<br />
        Are you sure?
      </v-card-text>
      <v-icon class="mx-auto" :icon="icon" size="48" />
      <v-row class="my-2">
        <v-btn
          @click="$emit('btnYesClick')"
          :disabled="loading"
          density="compact"
          class="mx-auto"
          width="7.5rem"
          color="info"
          text="yes"
        />
        <v-btn
          @click="() => (model = false)"
          :disabled="loading"
          density="compact"
          class="mx-auto"
          width="7.5rem"
          color="error"
          text="no"
        />
      </v-row>
    </v-card>
  </v-dialog>
</template>
