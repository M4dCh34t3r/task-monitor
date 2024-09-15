<template>
  <div class="justify-space-evenly align-center flex-column d-flex h-100">
    <v-card class="text-center" to="/Projects" width="160" hover>
      <v-card-title>PROJECTS</v-card-title>
      <v-icon size="48">{{ 'mdi-file-sign' }}</v-icon>
      <v-card-text>Manage projects</v-card-text>
    </v-card>

    <v-card class="text-center" to="/Tasks" width="160" hover>
      <v-card-title>TASKS</v-card-title>
      <v-icon size="48">{{ 'mdi-calendar' }}</v-icon>
      <v-card-text>Manage tasks</v-card-text>
    </v-card>

    <div class="position-absolute text-disabled bottom-0 right-0 text-end ma-2">
      © 2024<br />Pedro Andrade
    </div>
  </div>
</template>
