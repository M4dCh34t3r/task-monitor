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
        <v-icon ref="cpnIcon" size="24">{{ 'mdi-delete' }}</v-icon>
      </v-btn>
    </template>
  </v-tooltip>
</template>

<style scoped>
.animate {
  animation: animation 1s ease-out;
}

@keyframes animation {
  from {
    transform: rotateY(720deg);
  }
  to {
    transform: rotateY(0);
  }
}
</style>
