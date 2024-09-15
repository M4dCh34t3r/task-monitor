import * as t from 'io-ts';

const dateTimeType = t.union([t.number, t.string]);
const dateTimeOptionalType = t.union([t.number, t.string, t.null, t.undefined]);
const unknownRecordOptionalType = t.union([t.UnknownRecord, t.null, t.undefined]);

const dtos = {
  authRequestDTOType: t.type({
    userName: t.string,
    password: t.string
  }),
  clientSetupDTOType: t.type({
    ready: t.boolean,
    modalDelay: t.number,
    responseDelay: t.number
  }),
  contextResultExceptionDTOType: t.type({
    text: t.string
  }),
  jwtClaimsDTOType: t.type({
    nbf: t.number,
    exp: t.number,
    iat: t.number,
    uid: t.string,
    unm: t.string,
    adm: t.boolean
  }),
  serviceExceptionDTOType: t.type({
    theme: t.number,
    title: t.string,
    text: t.string
  })
};

const models = {
  collaboratorType: t.type({
    id: t.string,
    name: t.string,
    createdAt: dateTimeType,
    updatedAt: dateTimeOptionalType,
    deletedAt: dateTimeOptionalType
  }),
  projectType: t.type({
    id: t.string,
    name: t.string,
    createdAt: dateTimeType,
    updatedAt: dateTimeOptionalType,
    deletedAt: dateTimeOptionalType,
    tasks: t.UnknownArray
  }),
  taskType: t.type({
    id: t.string,
    projectId: t.string,
    name: t.string,
    description: t.string,
    createdAt: dateTimeType,
    updatedAt: dateTimeOptionalType,
    deletedAt: dateTimeOptionalType,
    timeTrackers: t.UnknownArray,
    project: unknownRecordOptionalType
  }),
  timeTrackerType: t.type({
    id: t.string,
    taskId: t.string,
    collaboratorId: t.string,
    startDate: dateTimeOptionalType,
    endDate: dateTimeOptionalType,
    task: unknownRecordOptionalType,
    collaborator: unknownRecordOptionalType
  }),
  userType: t.type({
    id: t.string,
    userName: t.string,
    password: t.string,
    createdAt: dateTimeType,
    updatedAt: dateTimeOptionalType,
    deletedAt: dateTimeOptionalType
  })
};

const assertMapping: Record<string, (key: string, obj: any) => any> = {
  authRequestDTO: (key, obj) => assert(dtos.authRequestDTOType, 'AuthRequestDTO', obj[key]),
  clientSetupDTO: (key, obj) => assert(dtos.clientSetupDTOType, 'ClientSetupDTO', obj[key]),
  contextResultExceptionDTO: (key, obj) =>
    assert(dtos.contextResultExceptionDTOType, 'ContextResultExceptionDTO', obj[key]),
  jwtClaimsDTO: (key, obj) => assert(dtos.jwtClaimsDTOType, 'JwtClaimsDTO', obj[key]),
  serviceExceptionDTO: (key, obj) =>
    assert(dtos.serviceExceptionDTOType, 'ServiceExceptionDTO', obj[key]),

  collaborator: (key, obj) => assert(models.collaboratorType, 'Collaborator', obj[key]),
  project: (key, obj) => assert(models.projectType, 'Project', obj[key]),
  task: (key, obj) => assert(models.taskType, 'Task', obj[key]),
  timeTracker: (key, obj) => assert(models.timeTrackerType, 'TimeTracker', obj[key]),
  user: (key, obj) => assert(models.userType, 'User', obj[key])
};

export function assertType<T>(obj: any): T {
  const key = Object.keys(obj)[0];

  if (import.meta.env.PROD) return obj[key];

  const mapping = assertMapping[key];
  return mapping ? mapping(key, obj) : ignore(key, obj[key]);
}

function assert<T>(typeIO: t.Type<T>, typeStr: string, obj: any) {
  const generateMismatchMessage = (obj: any): string => {
    let str = `An object of t.type\n\n  interface ${typeStr} {`;
    Object.keys(obj).forEach((key) => (str += `\n    ${key}: ${typeof obj[key]};`));
    return str + `\n  }\n\nDoesn't match "${typeStr}"`;
  };

  if (Array.isArray(obj))
    if (obj.length === 0 || typeIO.decode(obj[0])._tag === 'Right') return obj as T;
    else alert(generateMismatchMessage(obj[0]));

  if (typeIO.decode(obj)._tag === 'Right') return obj as T;
  else alert(generateMismatchMessage(obj));

  return obj as T;
}

function ignore<T>(key: string, obj: any) {
  alert(`The typings of "${key}" couldn't be validated`);
  return obj as T;
}
