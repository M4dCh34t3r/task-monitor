<script setup lang="ts">
import { ref } from 'vue';
import { VIcon } from 'vuetify/components';

const cpnIcon = ref<typeof VIcon>();

const emit = defineEmits<{
  (e: 'click'): void;
}>();

function click() {
  const el = cpnIcon.value?.$el;
  el.classList.add('animate');
  el.addEventListener('animationend', () => el.classList.remove('animate'));
  emit('click');
}

defineProps<{
  color?: string;
  disabled?: boolean;
  location?: 'top' | 'bottom' | 'left' | 'right';
  text: string;
}>();
</script>

<template>
  <v-tooltip :location="location || 'left'" :text="text">
    <template #activator="{ props }">
      <v-btn
        :color="color || 'grey'"
        :disabled="disabled"
        @click="click"
        size="x-small"
        v-bind="props"
        class="ma-1"
        icon
      >
        <v-icon ref="cpnIcon" size="24">{{ 'mdi-pencil' }}</v-icon>
      </v-btn>
    </template>
  </v-tooltip>
</template>

<style scoped>
.animate {
  animation: animation 1s ease-out;
}

@keyframes animation {
  0% {
    transform: translateX(0, 0);
  }
  12.5% {
    transform: translateX(-2px);
  }
  25% {
    transform: translateX(2px);
  }
  37.5% {
    transform: translateX(-2px);
  }
  50% {
    transform: translateX(2px);
  }
  62.5% {
    transform: translateX(-2px);
  }
  75% {
    transform: translateX(2px);
  }
  87.5% {
    transform: translateX(-2px);
  }
  100% {
    transform: translateX(0);
  }
}
</style>
