<script setup lang="ts">
import FrmLogin from '@/components/forms/FrmLogin.vue';
import { useConfigStore } from '@/stores';
import { storeToRefs } from 'pinia';
import { onMounted, ref } from 'vue';

const imgSlideSrc = [
  'banners/0.jpg',
  'banners/1.jpg',
  'banners/2.jpg',
  'banners/3.jpg',
  'banners/4.jpg',
  'banners/5.jpg'
];

const imgSlideSrcIndex = ref<number>(0);
const { inMobileMode } = storeToRefs(useConfigStore());

onMounted(() => {
  const updateImgSlideSrcIndex = () => {
    imgSlideSrcIndex.value = (imgSlideSrcIndex.value + 1) % imgSlideSrc.length;
    setTimeout(() => updateImgSlideSrcIndex(), 2500);
  };
  updateImgSlideSrcIndex();
});
</script>

<template>
  <div class="login-view">
    <v-card class="crd-title">
      <img src="/title.svg" width="100%" />
    </v-card>

    <FrmLogin v-if="inMobileMode" class="frm-login" />

    <v-card class="crd-desktop" v-else>
      <v-fade-transition leave-absolute>
        <v-img :src="imgSlideSrc[imgSlideSrcIndex]" :key="imgSlideSrcIndex" class="img-login" />
      </v-fade-transition>
      <FrmLogin class="frm-login" />
    </v-card>

    <!--
      <div class="div-links">
        <v-label text="Acesse nossas redes sociais!" />
        <v-row justify="space-between" class="my-2">
          <v-btn
            href="https://www.facebook.com/your-handler"
            icon="mdi-facebook"
            density="compact"
            rel="noreferrer"
            target="_blank"
            variant="plain"
          />
          <v-btn
            href="https://www.instagram.com/your-handler"
            icon="mdi-instagram"
            density="compact"
            rel="noreferrer"
            target="_blank"
            variant="plain"
          />
          <v-btn
            href="https://www.linkedin.com/company/your-handler"
            icon="mdi-linkedin"
            density="compact"
            rel="noreferrer"
            target="_blank"
            variant="plain"
          />
          <v-btn
            href="https://www.twitter.com/your-handler"
            icon="mdi-twitter"
            density="compact"
            rel="noreferrer"
            target="_blank"
            variant="plain"
          />
        </v-row>
      </div>
    -->
  </div>
</template>

<style scoped>
.crd-desktop {
  height: 420px;
  width: 1080px;
}

.crd-desktop {
  height: 65dvh;
  width: 75dvw;
}
.crd-desktop .frm-login {
  width: calc(100% - 65dvh - 32px);
  transform: translateY(-50%);
  position: absolute;
  right: 16px;
  top: 50%;
}

.crd-title {
  min-width: 320px;
  margin: 4dvh 0;
  width: 66.66%;
  padding: 8px;
}

.div-links {
  position: absolute;
  max-width: 560px;
  width: 100%;
  bottom: 0;
}

.frm-login {
  width: calc(100dvw - 32px);
}

.img-login {
  position: absolute;
  height: 65dvh;
  width: 65dvh;
}

.img-title {
  max-width: calc(100dvw - 64px);
  width: 125dvh;
  margin: 16px;
}

.login-view {
  flex-direction: column;
  justify-content: start;
  align-items: center;
  text-align: center;
  display: flex;
  height: 100%;
}
</style>
