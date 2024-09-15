<script setup lang="ts">
import { RouterView } from 'vue-router';
import { computed, onBeforeUnmount, onMounted } from 'vue';
import { storeToRefs } from 'pinia';
import { useConfigStore } from './stores/config';
import OverlayStoreDialogs from './components/misc/OverlayStoreDialogs.vue';
import OverlayStoreSnackbars from './components/misc/OverlayStoreSnackbars.vue';
import { useSetupStore } from './stores';
import ApbDesktop from './components/appBars/ApbDesktop.vue';
import ApbMobile from './components/appBars/ApbMobile.vue';
import CrdServerNotRunning from './components/cards/crdServerNotRunning.vue';
import OverlayStoreNavigationDrawers from './components/misc/OverlayStoreNavigationDrawers.vue';

const setupStore = useSetupStore();
const { inDarkMode, inMobileMode } = storeToRefs(useConfigStore());

const appTheme = computed(() => (inDarkMode.value ? 'defaultDark' : 'defaultLight'));

function onWindowResize() {
  if (inMobileMode.value && window.innerWidth > 1024) inMobileMode.value = false;
  else if (!inMobileMode.value && window.innerWidth <= 1024) inMobileMode.value = true;
}

onBeforeUnmount(() => window.removeEventListener('resize', onWindowResize));

onMounted(async () => {
  document.addEventListener('contextmenu', (e) => e.preventDefault(), false);
  window.addEventListener('resize', onWindowResize);
  await setupStore.fetchData();
});
</script>

<template>
  <v-app :theme="appTheme">
    <v-fade-transition mode="out-in">
      <v-main v-if="setupStore.ready">
        <v-fade-transition>
          <div v-if="inMobileMode" >
            <OverlayStoreNavigationDrawers />
            <ApbMobile />
          </div>
          <ApbDesktop v-else />
        </v-fade-transition>

        <OverlayStoreDialogs />

        <OverlayStoreSnackbars />

        <router-view v-slot="{ Component }">
          <v-fade-transition mode="out-in">
            <component :is="Component" />
          </v-fade-transition>
        </router-view>
      </v-main>

      <v-main v-else-if="setupStore.ready === false">
        <CrdServerNotRunning />
      </v-main>

    </v-fade-transition>
  </v-app>
</template>
