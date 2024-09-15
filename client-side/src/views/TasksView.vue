<script setup lang="ts">
import {
  extractDateHours,
  extractTimeLeftMessage,
  formatMillisecondsToTime,
  getEndOfToday,
  getStartOfToday,
  isThisMonth
} from '@/utils/dateTimeUtil';
import BtnExcluir from '@/components/buttons/BtnDelete.vue';
import BtnEdit from '@/components/buttons/BtnEdit.vue';
import CdaConfirmCancel from '@/components/cardActions/CdaConfirmCancel.vue';
import DlgConfirmation from '@/components/dialogs/DlgConfirmation.vue';
import DlgWindow from '@/components/dialogs/DlgWindow.vue';
import { assertType } from '@/helpers/typeAssertHelper';
import type { Collaborator, Project, Task, TimeTracker } from '@/typings';
import { HttpStatusCode, type AxiosInstance } from 'axios';
import { computed, inject, onMounted, onUnmounted, ref, watch } from 'vue';
import { useCrudStore, useOverlayStore } from '@/stores';
import BtnAlarm from '@/components/buttons/BtnAlarm.vue';

let intervalId = -1;

const acpAddTimeTrackerCollaborator = ref<Collaborator>();
const btnAddTimeTrackerConfirm = ref<boolean>(false);
const acpCrudProject = ref<Project>();
const acpFilterCollaboratorId = ref<string>();
const acpFilterProjectId = ref<string>();
const axios = inject<AxiosInstance>('axios')!;
const btnTaskLoading = ref<boolean>(false);
const crudStore = useCrudStore();
const dlgAddTask = ref<boolean>(false);
const dlgDeleteTask = ref<boolean>(false);
const dlgEditTask = ref<boolean>(false);
const dlgManageTimeTrackers = ref<boolean>(false);
const dlgAddTimeTracker = ref<boolean>(false);
const jsonCollaborators = ref<Collaborator[]>([]);
const jsonProjects = ref<Project[]>([]);
const jsonTasks = ref<Task[]>();
const jsonTimeTrackers = ref<TimeTracker[]>([]);
const overlayStore = useOverlayStore();
const txfAddTaskDescription = ref<string>('');
const txfAddTaskName = ref<string>('');
const txfAddTimeTrackerTimeZoneId = ref<string>('');
const txfEditTaskDescription = ref<string>('');
const txfEditTaskName = ref<string>('');
const spentToday = ref<string>('--:--');
const spentThisMonth = ref<string>('--:--');

const btnAddProjectDisabled = computed(
  () =>
    txfAddTaskDescription.value.length === 0 ||
    txfAddTaskName.value.length === 0 ||
    !acpCrudProject.value
);
const btnAddTimeTrackerConfirmDisabled = computed(
  () => txfAddTimeTrackerTimeZoneId.value.length === 0 || !acpAddTimeTrackerCollaborator.value
);
const btnEditTaskDisabled = computed(
  () =>
    txfEditTaskDescription.value.length === 0 ||
    txfEditTaskName.value.length === 0 ||
    !acpCrudProject.value
);

async function btnAddProjectConfirmClick() {
  const data: Partial<Task> = {
    description: txfAddTaskDescription.value,
    projectId: acpCrudProject.value!.id,
    name: txfAddTaskName.value
  };

  btnTaskLoading.value = true;

  try {
    const res = await axios.post('Task', data);
    if (res?.status === HttpStatusCode.Created) {
      const task = assertType<Task>({ task: res.data });
      task.project = acpCrudProject.value;
      jsonTasks.value!.push(task);
      overlayStore.showSuccessMessageDialog(
        'Task created',
        `The task "${task.name}" was created successfuly`
      );
    }
  } finally {
    btnTaskLoading.value = false;
    dlgAddTask.value = false;
  }
}

function btnAddTaskClick() {
  acpCrudProject.value = undefined;
  txfAddTaskDescription.value = '';
  txfAddTaskName.value = '';
  dlgAddTask.value = true;
}

function btnAddTimeTrackerClick() {
  txfAddTimeTrackerTimeZoneId.value = Intl.DateTimeFormat().resolvedOptions().timeZone;
  acpAddTimeTrackerCollaborator.value = undefined;
  dlgAddTimeTracker.value = true;
}

async function btnAddTimeTrackerConfirmClick() {
  const data: Partial<TimeTracker> = {
    collaboratorId: acpAddTimeTrackerCollaborator.value!.id,
    timeZoneId: txfAddTimeTrackerTimeZoneId.value,
    taskId: crudStore.task?.id
  };

  btnAddTimeTrackerConfirm.value = true;

  try {
    const res = await axios.post('TimeTracker', data);
    if (res?.status === HttpStatusCode.Created) {
      const timeTracker = assertType<TimeTracker>({ timeTracker: res.data });
      timeTracker.collaborator = acpAddTimeTrackerCollaborator.value;
      timeTracker.task = crudStore.task;
      jsonTimeTrackers.value.push(timeTracker);
    }
  } finally {
    btnAddTimeTrackerConfirm.value = false;
    dlgAddTimeTracker.value = false;
  }
}

function btnDeleteTaskClick(task: Task) {
  crudStore.task = task;
  dlgDeleteTask.value = true;
}

async function btnDeleteTaskConfirmClick() {
  const dataId = crudStore.task?.id;

  btnTaskLoading.value = true;

  try {
    const res = await axios.post(`Task/Delete/${dataId}`);
    if (res?.status === HttpStatusCode.NoContent) {
      const index = jsonTasks.value!.findIndex((t) => t.id === dataId);
      if (index !== -1) jsonTasks.value!.splice(index, 1);
    }
  } finally {
    btnTaskLoading.value = false;
    dlgDeleteTask.value = false;
  }
}

function btnEditTaskClick(task: Task) {
  crudStore.task = task;

  txfEditTaskDescription.value = task.description;
  acpCrudProject.value = task.project;
  txfEditTaskName.value = task.name;

  dlgEditTask.value = true;
}

async function btnEditTaskConfirmClick() {
  const dataId = crudStore.task?.id;
  const data: Partial<Task> = {
    description: txfEditTaskDescription.value,
    projectId: acpCrudProject.value!.id,
    name: txfEditTaskName.value,
    id: dataId
  };

  btnTaskLoading.value = true;

  try {
    const res = await axios.put(`Task/${dataId}`, data);
    if (res?.status === HttpStatusCode.Ok) {
      const task = assertType<Task>({ task: res.data });
      const index = jsonTasks.value!.findIndex((t) => t.id === task.id);
      if (index !== -1) {
        task.project = crudStore.task!.project;
        jsonTasks.value![index] = task;
        overlayStore.showSuccessMessageDialog(
          'Task updated',
          `The task "${task.name}" was updated successfuly`
        );
      }
    }
  } finally {
    btnTaskLoading.value = false;
    dlgEditTask.value = false;
  }
}

async function btnEndTimeTrackerClick(timeTracker: TimeTracker) {
  const data = {
    ...timeTracker
  };

  // SETTING THE DATE VALUE TO THE LOCAL TIMEZONE ONE
  // (IT'S BEING PROPERLY ADJUSTED ON THE SERVER-SIDE)
  data.endDate = new Date().toISOString();

  // ALTERNATIVE ENDPOINT FOR A BETTER SERVER DATE HANDLING
  //const res = await axios.post(`TimeTracker/EndDate/${timeTracker.id}`);
  const res = await axios.put(`TimeTracker/${data.id}`, data);
  if (res?.status === HttpStatusCode.Ok) {
    assertType<TimeTracker>({ timeTracker: res.data });
    const index = jsonTimeTrackers.value!.findIndex((t) => t.id === data.id);
    if (index !== -1) {
      const date = new Date();
      jsonTimeTrackers.value[index].endDate = new Date(
        date.getTime() + date.getTimezoneOffset() * 60 * 1000
      ).toISOString();
    }
  }
}

async function btnManageTrackingsClick(task: Task) {
  const queryParams = {
    includeCollaborators: true
  };

  overlayStore.showLoadingDialog();

  try {
    const res = await axios.get(`TimeTracker/Task/${task.id}`, { params: queryParams });
    if (res?.status === HttpStatusCode.Ok) {
      const timeTrackers = assertType<TimeTracker[]>({ timeTracker: res.data });
      jsonTimeTrackers.value = timeTrackers;
      crudStore.task = task;
      dlgManageTimeTrackers.value = true;
    }
  } finally {
    overlayStore.hideLoadingDialog();
  }
}

async function btnStartTimeTrackerClick(timeTracker: TimeTracker) {
  const data = {
    ...timeTracker
  };

  // SETTING THE DATE VALUE TO THE LOCAL TIMEZONE ONE
  // (IT'S BEING PROPERLY ADJUSTED ON THE SERVER-SIDE)
  data.startDate = new Date().toISOString();
  data.collaborator = undefined;
  data.task = undefined;

  // ALTERNATIVE ENDPOINT FOR A BETTER SERVER DATE HANDLING
  //const res = await axios.post(`TimeTracker/StartDate/${timeTracker.id}`);
  const res = await axios.put(`TimeTracker/${data.id}`, data);
  if (res?.status === HttpStatusCode.Ok) {
    assertType<TimeTracker>({ timeTracker: res.data });
    const index = jsonTimeTrackers.value!.findIndex((t) => t.id === data.id);
    if (index !== -1) {
      const date = new Date();
      jsonTimeTrackers.value[index].startDate = new Date(
        date.getTime() + date.getTimezoneOffset() * 60 * 1000
      ).toISOString();
    }
  }
}

function onInterval() {
  const startOfToday = getStartOfToday();
  const endOfToday = getEndOfToday();

  const timeSpentToday = jsonTimeTrackers.value.map((t) => {
    if (typeof t.startDate === 'string') {
      const startDate = new Date(t.startDate);
      const startDateUtcMs = startDate.getTime() - startDate.getTimezoneOffset() * 60 * 1000;
      const endDate = typeof t.endDate === 'string' ? new Date(t.endDate) : new Date();
      const endDateUtcMs = endDate.getTime() - endDate.getTimezoneOffset() * 60 * 1000;

      const overlapStart = Math.max(startOfToday, startDateUtcMs);
      const overlapEnd = Math.min(endOfToday, endDateUtcMs);

      if (overlapStart < overlapEnd) return overlapEnd - overlapStart;
    }
    return 0;
  });
  spentToday.value = formatMillisecondsToTime(
    timeSpentToday.reduce((accumulator, currentValue) => accumulator + currentValue, 0)
  );

  const timeSpentThisMonth = jsonTimeTrackers.value.map((t) => {
    if (typeof t.startDate === 'string' && isThisMonth(t.startDate)) {
      const startDate = new Date(t.startDate);
      const startDateUtcMs = startDate.getTime() - startDate.getTimezoneOffset() * 60 * 1000;
      const endDate = typeof t.endDate === 'string' ? new Date(t.endDate) : new Date();
      const endDateUtcMs = endDate.getTime() - endDate.getTimezoneOffset() * 60 * 1000;

      return typeof t.endDate === 'string'
        ? endDateUtcMs - startDateUtcMs
        : Date.now() - startDateUtcMs;
    }
    return 0;
  });
  spentThisMonth.value = formatMillisecondsToTime(
    timeSpentThisMonth.reduce((accumulator, currentValue) => accumulator + currentValue, 0)
  );
}

onMounted(async () => {
  const queryParamsTasks = {
    includeProjects: true
  };
  const resTasks = await axios.get('Task', { params: queryParamsTasks });
  if (resTasks?.status === HttpStatusCode.Ok)
    jsonTasks.value = assertType<Task[]>({ task: resTasks.data });

  const resProjects = await axios.get('Project');
  if (resProjects?.status === HttpStatusCode.Ok)
    jsonProjects.value = assertType<Project[]>({ project: resProjects.data });

  const resCollaboratos = await axios.get('Collaborator');
  if (resCollaboratos?.status === HttpStatusCode.Ok)
    jsonCollaborators.value = assertType<Collaborator[]>({ collaborator: resCollaboratos.data });

  intervalId = setInterval(onInterval, 1000);
});

onUnmounted(() => {
  clearInterval(intervalId);
  crudStore.task = undefined;
});

watch([acpFilterProjectId, acpFilterCollaboratorId], async ([projectId, collaboratorId]) => {
  const queryParams = {
    includeProjects: true,
    collaboratorId,
    projectId
  };

  jsonTasks.value = undefined;

  try {
    const res = await axios.get(`Task/Filter`, { params: queryParams });
    if (res?.status === HttpStatusCode.Ok) {
      const tasks = assertType<Task[]>({ task: res.data });
      jsonTasks.value = tasks;
    }
  } catch {
    jsonTasks.value = [];
  }
});

watch([dlgDeleteTask, dlgEditTask], ([valDelete, valEdit]) => {
  if (!valDelete && !valEdit) crudStore.task = undefined;
});
</script>

<template>
  <v-card class="view-tasks">
    <DlgConfirmation
      @btn-yes-click="btnDeleteTaskConfirmClick"
      text="The selected task will be deleted"
      :loading="btnTaskLoading"
      v-model="dlgDeleteTask"
      title="DELETE TASK"
      icon="mdi-calendar"
    />

    <DlgWindow
      v-model="dlgManageTimeTrackers"
      title="MANAGE TIME TRACKERS"
      btn-toggle-disabled
      max-width="720"
    >
      <v-dialog v-model="dlgAddTimeTracker" max-width="480">
        <v-card>
          <v-card-title class="bg-secondary"> Add time tracker </v-card-title>

          <v-autocomplete
            v-model="acpAddTimeTrackerCollaborator"
            :items="jsonCollaborators"
            label="Collaborator"
            class="mx-2 mt-2"
            return-object
          />

          <v-text-field
            v-model="txfAddTimeTrackerTimeZoneId"
            label="Time tracker timezone ID"
            class="mx-2 mt-2"
            disabled
          />

          <CdaConfirmCancel
            :btn-confirm-disabled="btnAddTimeTrackerConfirmDisabled"
            @btn-cancel-click="() => (dlgAddTimeTracker = false)"
            @btn-confirm-click="btnAddTimeTrackerConfirmClick"
            :btn-confirm-loading="btnAddTimeTrackerConfirm"
          />
        </v-card>
      </v-dialog>

      <div v-if="jsonTimeTrackers.length">
        <v-table>
          <thead>
            <tr>
              <th>COLLABORATOR</th>
              <th>START DATE</th>
              <th>END DATE</th>
              <th class="text-center">TIME DECURRED</th>
              <th style="width: 112px" class="text-center">ACTIONS</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="timeTracker in jsonTimeTrackers" :key="timeTracker.id">
              <td>
                {{ timeTracker.collaborator?.name }}
              </td>
              <td>
                {{
                  typeof timeTracker.startDate === 'string'
                    ? extractDateHours(timeTracker.startDate, true)
                    : 'N/A'
                }}
              </td>
              <td>
                {{
                  typeof timeTracker.endDate === 'string'
                    ? extractDateHours(timeTracker.endDate, true)
                    : 'N/A'
                }}
              </td>
              <td class="text-center">
                {{
                  typeof timeTracker.startDate === 'string'
                    ? extractTimeLeftMessage(
                        timeTracker.startDate,
                        typeof timeTracker.endDate === 'string' ? timeTracker.endDate : new Date()
                      )
                    : 'N/A'
                }}
              </td>
              <td>
                <v-btn
                  @click="btnStartTimeTrackerClick(timeTracker)"
                  class="my-2 mx-1"
                  color="success"
                  icon="mdi-play"
                  size="32"
                />
                <v-btn
                  @click="btnEndTimeTrackerClick(timeTracker)"
                  class="my-2 mx-1"
                  icon="mdi-stop"
                  color="error"
                  size="32"
                />
              </td>
            </tr>
          </tbody>
        </v-table>

        <v-card color="primary" class="ma-2 pa-2">
          <v-row justify="space-around" class="font-weight-bold text-h6">
            <v-col align="start">
              <v-fade-transition leave-absolute>
                <div :key="spentThisMonth">Time spent this month - {{ spentThisMonth }}</div>
              </v-fade-transition>
            </v-col>
            <v-col align="end">
              <v-fade-transition leave-absolute>
                <div :key="spentToday">Time spent today - {{ spentToday }}</div>
              </v-fade-transition>
            </v-col>
          </v-row>
        </v-card>
      </div>
      <div class="text-center my-2" v-else>There are no trackings in this project</div>
      <v-btn
        @click="btnAddTimeTrackerClick"
        text="add time tracker"
        append-icon="mdi-plus"
        color="secondary"
      />
    </DlgWindow>

    <DlgWindow v-model="dlgAddTask" btn-toggle-disabled title="ADD TASK" max-width="480">
      <v-text-field v-model="txfAddTaskName" class="mx-2 mt-2" label="Task name" />
      <v-textarea v-model="txfAddTaskDescription" label="Description" class="mx-2 mt-2" />
      <v-autocomplete
        v-model="acpCrudProject"
        :items="jsonProjects"
        class="mx-2 mt-2"
        label="Project"
        return-object
      />
      <CdaConfirmCancel
        @btn-cancel-click="() => (dlgAddTask = false)"
        @btn-confirm-click="btnAddProjectConfirmClick"
        :btn-confirm-disabled="btnAddProjectDisabled"
        :btn-confirm-loading="btnTaskLoading"
      />
    </DlgWindow>

    <DlgWindow v-model="dlgEditTask" btn-toggle-disabled title="EDIT TASK" max-width="480">
      <v-text-field v-model="txfEditTaskName" class="mx-2 mt-2" label="Task name" />
      <v-textarea v-model="txfEditTaskDescription" label="Description" class="mx-2 mt-2" />
      <v-autocomplete
        v-model="acpCrudProject"
        :items="jsonProjects"
        class="mx-2 mt-2"
        label="Project"
        return-object
      />
      <CdaConfirmCancel
        @btn-cancel-click="() => (dlgEditTask = false)"
        @btn-confirm-click="btnEditTaskConfirmClick"
        :btn-confirm-disabled="btnEditTaskDisabled"
        :btn-confirm-loading="btnTaskLoading"
      />
    </DlgWindow>

    <v-card-title class="text-center bg-secondary">TASKS</v-card-title>
    <v-row class="ma-1" no-gutters>
      <v-col align="center">
        <v-autocomplete
          prepend-inner-icon="mdi-file-sign"
          v-model="acpFilterProjectId"
          :items="jsonProjects"
          label="Project"
          class="ma-1"
          clearable
        />
      </v-col>
      <v-col>
        <v-autocomplete
          prepend-inner-icon="mdi-account"
          v-model="acpFilterCollaboratorId"
          :items="jsonCollaborators"
          label="Collaborator"
          class="ma-1"
          clearable
        />
      </v-col>
    </v-row>

    <v-fade-transition leave-absolute>
      <v-table v-if="jsonTasks">
        <thead>
          <tr>
            <th class="bg-primary text-start">ID</th>
            <th class="bg-primary text-center">NAME</th>
            <th class="bg-primary text-center">DESCRIPTION</th>
            <th class="bg-primary text-center">PROJECT NAME</th>
            <th class="bg-primary text-center">TIME SPENT</th>
            <th class="bg-primary text-center" style="width: 152px">ACTIONS</th>
          </tr>
        </thead>
        <v-fade-transition tag="tbody" group>
          <tr v-for="(task, index) in jsonTasks" :key="index">
            <td class="text-start">
              {{ task.id }}
            </td>
            <td class="text-center">
              {{ task.name }}
            </td>
            <td class="text-center">
              {{ task.description }}
            </td>
            <td class="text-center">
              {{ task.project?.name }}
            </td>
            <td></td>
            <td class="text-center">
              <BtnExcluir @click="btnDeleteTaskClick(task)" text="Delete task" />
              <BtnEdit @click="btnEditTaskClick(task)" text="Edit task" />
              <BtnAlarm @click="btnManageTrackingsClick(task)" text="Manage trackings" />
            </td>
          </tr>
        </v-fade-transition>
      </v-table>
      <v-skeleton-loader v-else />
    </v-fade-transition>
    <v-card-actions>
      <v-btn
        @click="btnAddTaskClick"
        :disabled="!jsonTasks"
        append-icon="mdi-plus"
        color="secondary"
        text="add task"
        class="mx-auto"
        variant="flat"
        width="15rem"
      />
    </v-card-actions>
  </v-card>
</template>

<style scoped>
.view-tasks {
  height: calc(100% - 16px - var(--v-compact-height));
  margin: 32px;
}

.v-table,
.v-skeleton-loader {
  background-color: rgb(var(--v-theme-mix));
  height: calc(100% - 144px);
}

@media (max-width: 1024px) {
  .view-tasks {
    border-radius: 0;
    height: 100%;
    margin: 0;
  }

  .v-table,
  .v-skeleton-loader {
    height: calc(100% - 144px);
  }
}
</style>
