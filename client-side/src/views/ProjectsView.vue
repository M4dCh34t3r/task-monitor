<script setup lang="ts">
import { extractDateHours } from '@/utils/dateTimeUtil';
import CdaConfirmCancel from '@/components/cardActions/CdaConfirmCancel.vue';
import DlgWindow from '@/components/dialogs/DlgWindow.vue';
import { assertType } from '@/helpers/typeAssertHelper';
import { useCrudStore, useOverlayStore } from '@/stores';
import type { Project } from '@/typings';
import { HttpStatusCode, type AxiosInstance } from 'axios';
import { computed, inject, onMounted, onUnmounted, ref } from 'vue';
import BtnExcluir from '@/components/buttons/BtnDelete.vue';
import BtnEdit from '@/components/buttons/BtnEdit.vue';
import DlgConfirmation from '@/components/dialogs/DlgConfirmation.vue';

const axios = inject<AxiosInstance>('axios')!;
const btnProjectLoading = ref<boolean>(false);
const crudStore = useCrudStore();
const dlgAddProject = ref<boolean>(false);
const dlgEditProject = ref<boolean>(false);
const dlgDeleteProject = ref<boolean>(false);
const jsonProjects = ref<Project[]>();
const overlayStore = useOverlayStore();
const txfAddProjectName = ref<string>('');
const txfEditProjectName = ref<string>('');

const btnAddProjectDisabled = computed(() => txfAddProjectName.value.length === 0);
const btnEditProjectDisabled = computed(() => txfEditProjectName.value.length === 0);

function btnAddProjectClick() {
  txfAddProjectName.value = '';
  dlgAddProject.value = true;
}

async function btnAddProjectConfirmClick() {
  const data: Partial<Project> = {
    name: txfAddProjectName.value
  };

  btnProjectLoading.value = true;

  try {
    const res = await axios.post('Project', data);
    if (res?.status === HttpStatusCode.Created) {
      const project = assertType<Project>({ project: res.data });
      jsonProjects.value!.push(project);
      overlayStore.showSuccessMessageDialog(
        'Project created',
        `The project "${project.name}" was added successfuly`
      );
    }
  } finally {
    btnProjectLoading.value = false;
    dlgAddProject.value = false;
  }
}

function btnDeleteProjectClick(project: Project) {
  crudStore.project = project;
  dlgDeleteProject.value = true;
}

async function btnDeleteProjectConfirmClick() {
  const dataId = crudStore.project?.id;

  btnProjectLoading.value = true;

  try {
    const res = await axios.post(`Project/Delete/${dataId}`);
    if (res?.status === HttpStatusCode.NoContent) {
      const index = jsonProjects.value!.findIndex((p) => p.id === dataId);
      if (index !== -1) jsonProjects.value!.splice(index, 1);
    }
  } finally {
    btnProjectLoading.value = false;
    dlgDeleteProject.value = false;
  }
}

function btnEditProjectClick(project: Project) {
  crudStore.project = project;

  txfEditProjectName.value = project.name;

  dlgEditProject.value = true;
}

async function btnEditProjectConfirmClick() {
  const dataId = crudStore.project?.id;
  const data: Partial<Project> = {
    name: txfEditProjectName.value,
    id: dataId
  };

  btnProjectLoading.value = true;

  try {
    const res = await axios.put(`Project/${dataId}`, data);
    if (res?.status === HttpStatusCode.Ok) {
      const project = assertType<Project>({ project: res.data });
      const index = jsonProjects.value!.findIndex((p) => p.id === project.id);
      if (index !== -1) {
        jsonProjects.value![index] = project;
        overlayStore.showSuccessMessageDialog(
          'Project updated',
          `The project "${project.name}" was updated successfuly`
        );
      }
    }
  } finally {
    btnProjectLoading.value = false;
    dlgEditProject.value = false;
  }
}

onMounted(async () => {
  const res = await axios.get('Project');
  if (res?.status === HttpStatusCode.Ok)
    jsonProjects.value = assertType<Project[]>({ project: res.data });
});

onUnmounted(() => (crudStore.project = undefined));
</script>

<template>
  <v-card class="view-projects">
    <DlgConfirmation
      @btn-yes-click="btnDeleteProjectConfirmClick"
      text="The selected project will be deleted"
      :loading="btnProjectLoading"
      v-model="dlgDeleteProject"
      title="DELETE PROJECT"
      icon="mdi-file-sign"
    />

    <DlgWindow v-model="dlgAddProject" btn-toggle-disabled title="ADD PROJECT" max-width="480">
      <v-text-field v-model="txfAddProjectName" label="Project name" class="mx-2 mt-2" />
      <CdaConfirmCancel
        @btn-cancel-click="() => (dlgAddProject = false)"
        @btn-confirm-click="btnAddProjectConfirmClick"
        :btn-confirm-disabled="btnAddProjectDisabled"
        :btn-confirm-loading="btnProjectLoading"
      />
    </DlgWindow>

    <DlgWindow v-model="dlgEditProject" btn-toggle-disabled title="EDIT PROJECT" max-width="480">
      <v-text-field v-model="txfEditProjectName" class="mx-2 mt-2" label="Project name" />
      <CdaConfirmCancel
        @btn-cancel-click="() => (dlgEditProject = false)"
        @btn-confirm-click="btnEditProjectConfirmClick"
        :btn-confirm-disabled="btnEditProjectDisabled"
        :btn-confirm-loading="btnProjectLoading"
      />
    </DlgWindow>

    <v-fade-transition leave-absolute>
      <v-table v-if="jsonProjects">
        <thead>
          <tr>
            <th class="bg-primary">ID</th>
            <th class="bg-primary">NAME</th>
            <th class="bg-primary">CREATED AT</th>
            <th class="bg-primary">UPDATED AT</th>
            <th class="bg-primary text-center">ACTIONS</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="project in jsonProjects" :key="project.id">
            <td>
              {{ project.id }}
            </td>
            <td>
              {{ project.name }}
            </td>
            <td>
              {{
                typeof project.createdAt === 'string'
                  ? extractDateHours(project.createdAt, true)
                  : 'N/A'
              }}
            </td>
            <td>
              {{
                typeof project.updatedAt === 'string'
                  ? extractDateHours(project.updatedAt, true)
                  : 'N/A'
              }}
            </td>
            <td class="text-center">
              <BtnExcluir @click="btnDeleteProjectClick(project)" text="Delete project" />
              <BtnEdit @click="btnEditProjectClick(project)" text="Edit project" />
            </td>
          </tr>
        </tbody>
      </v-table>
      <v-skeleton-loader v-else />
    </v-fade-transition>
    <v-card-actions>
      <v-btn
        @click="btnAddProjectClick"
        :disabled="!jsonProjects"
        append-icon="mdi-plus"
        text="add project"
        color="secondary"
        class="mx-auto"
        variant="flat"
        width="15rem"
      />
    </v-card-actions>
  </v-card>
</template>

<style scoped>
.view-projects {
  height: calc(100% - 16px - var(--v-compact-height));
  margin: 32px;
}

.v-table,
.v-skeleton-loader {
  background-color: rgb(var(--v-theme-mix));
  height: calc(100% - 52px);
}

@media (max-width: 1024px) {
  .view-projects {
    border-radius: 0;
    height: 100%;
    margin: 0;
  }

  .v-table,
  .v-skeleton-loader {
    height: calc(100% - 52px);
  }
}
</style>
