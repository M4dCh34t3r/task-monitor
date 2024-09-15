<script setup lang="ts">
defineProps<{
  btnCancelDisabled?: boolean;
  btnConfirmDisabled?: boolean;
  btnConfirmLoading?: boolean;
  density?: 'comfortable' | 'compact' | 'default';
}>();
</script>

<template>
  <v-card-actions density="compact" class="my-auto">
    <v-btn
      @click="() => $emit('btnConfirmClick')"
      :density="density || 'default'"
      :disabled="btnConfirmDisabled"
      :loading="btnConfirmLoading"
      class="ml-auto"
      text="confirm"
      variant="flat"
      width="7.5rem"
      color="info"
    />
    <v-btn
      @click="() => $emit('btnCancelClick')"
      :density="density || 'default'"
      :disabled="btnCancelDisabled"
      variant="flat"
      width="7.5rem"
      color="error"
      text="cancel"
    />
  </v-card-actions>
</template>
