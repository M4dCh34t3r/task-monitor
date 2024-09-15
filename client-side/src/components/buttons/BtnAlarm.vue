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
        <v-icon ref="cpnIcon" size="24">{{ 'mdi-alarm' }}</v-icon>
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
    transform: translateX(0);
  }
  10% {
    transform: translateX(2px);
  }
  20% {
    transform: translateX(-2px);
  }
  30% {
    transform: translateX(2px);
  }
  40% {
    transform: translateX(-2px);
  }
  50% {
    transform: translateX(2px);
  }
  60% {
    transform: translateX(-2px);
  }
  70% {
    transform: translateX(2px);
  }
  80% {
    transform: translateX(-2px);
  }
  90% {
    transform: translateX(2px);
  }
  100% {
    transform: translateX(0);
  }
}
</style>
