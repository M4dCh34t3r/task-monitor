<script setup lang="ts">
function btnReloadPageClick() {
  setTimeout(() => location.reload(), 500);
}
</script>

<template>
    <v-card class="text-center mx-auto my-16 pa-4" max-width="480">
        <v-icon class="icn-monitor" size="64">{{ 'mdi-monitor' }}</v-icon>
        <v-card-title>The app's server is not running as expected</v-card-title>
        <v-card-subtitle>Check if the "client setup" service is avaliable</v-card-subtitle>
        <v-btn @click="btnReloadPageClick" text="reload page" class="mt-4" color="mix" />
    </v-card>
</template>

<style scoped>
.icn-monitor {
  animation: icn-monitor-sway 2.5s infinite ease-in-out;
  transform-origin: bottom center;
}

@keyframes icn-monitor-sway {
  0% {
    transform: rotate(-10deg);
  }
  50% {
    transform: rotate(10deg);
  }
  100% {
    transform: rotate(-10deg);
  }
}
</style>
