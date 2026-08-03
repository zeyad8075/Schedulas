CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE TABLE institutions (
    id uuid NOT NULL,
    name text NOT NULL,
    type text NOT NULL,
    timezone text NOT NULL,
    logo_url text,
    is_suspended boolean NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone,
    deleted_at timestamp with time zone,
    created_by uuid,
    updated_by uuid,
    CONSTRAINT "PK_institutions" PRIMARY KEY (id)
);

CREATE TABLE profiles (
    id uuid NOT NULL,
    full_name text NOT NULL,
    email text NOT NULL,
    phone_number text,
    role text NOT NULL,
    institution_id uuid,
    department_id uuid,
    is_active boolean NOT NULL,
    preferred_theme text NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone,
    deleted_at timestamp with time zone,
    created_by uuid,
    updated_by uuid,
    CONSTRAINT "PK_profiles" PRIMARY KEY (id)
);

CREATE TABLE rule_definitions (
    id uuid NOT NULL,
    institution_id uuid NOT NULL,
    scope_level text NOT NULL,
    scope_id uuid NOT NULL,
    rule_type text NOT NULL,
    parameters jsonb NOT NULL,
    priority integer NOT NULL,
    is_active boolean NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone,
    deleted_at timestamp with time zone,
    created_by uuid,
    updated_by uuid,
    CONSTRAINT "PK_rule_definitions" PRIMARY KEY (id)
);

CREATE TABLE academic_terms (
    id uuid NOT NULL,
    institution_id uuid NOT NULL,
    name text NOT NULL,
    start_date date NOT NULL,
    end_date date NOT NULL,
    is_active boolean NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone,
    deleted_at timestamp with time zone,
    created_by uuid,
    updated_by uuid,
    CONSTRAINT "PK_academic_terms" PRIMARY KEY (id),
    CONSTRAINT ck_academic_terms_dates CHECK (end_date > start_date),
    CONSTRAINT "FK_academic_terms_institutions_institution_id" FOREIGN KEY (institution_id) REFERENCES institutions (id) ON DELETE CASCADE
);

CREATE TABLE departments (
    id uuid NOT NULL,
    institution_id uuid NOT NULL,
    name text NOT NULL,
    is_active boolean NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone,
    deleted_at timestamp with time zone,
    created_by uuid,
    updated_by uuid,
    CONSTRAINT "PK_departments" PRIMARY KEY (id),
    CONSTRAINT "FK_departments_institutions_institution_id" FOREIGN KEY (institution_id) REFERENCES institutions (id) ON DELETE CASCADE
);

CREATE TABLE device_tokens (
    id uuid NOT NULL,
    profile_id uuid NOT NULL,
    token text NOT NULL,
    platform text NOT NULL,
    is_active boolean NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone,
    deleted_at timestamp with time zone,
    created_by uuid,
    updated_by uuid,
    CONSTRAINT "PK_device_tokens" PRIMARY KEY (id),
    CONSTRAINT "FK_device_tokens_profiles_profile_id" FOREIGN KEY (profile_id) REFERENCES profiles (id) ON DELETE CASCADE
);

CREATE TABLE parents (
    id uuid NOT NULL,
    profile_id uuid NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone,
    deleted_at timestamp with time zone,
    created_by uuid,
    updated_by uuid,
    CONSTRAINT "PK_parents" PRIMARY KEY (id),
    CONSTRAINT "FK_parents_profiles_profile_id" FOREIGN KEY (profile_id) REFERENCES profiles (id) ON DELETE CASCADE
);

CREATE TABLE students (
    id uuid NOT NULL,
    profile_id uuid NOT NULL,
    institution_id uuid NOT NULL,
    student_number text,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone,
    deleted_at timestamp with time zone,
    created_by uuid,
    updated_by uuid,
    CONSTRAINT "PK_students" PRIMARY KEY (id),
    CONSTRAINT "FK_students_profiles_profile_id" FOREIGN KEY (profile_id) REFERENCES profiles (id) ON DELETE CASCADE
);

CREATE TABLE teachers (
    id uuid NOT NULL,
    profile_id uuid NOT NULL,
    institution_id uuid NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone,
    deleted_at timestamp with time zone,
    created_by uuid,
    updated_by uuid,
    CONSTRAINT "PK_teachers" PRIMARY KEY (id),
    CONSTRAINT "FK_teachers_profiles_profile_id" FOREIGN KEY (profile_id) REFERENCES profiles (id) ON DELETE CASCADE
);

CREATE TABLE holidays (
    id uuid NOT NULL,
    institution_id uuid NOT NULL,
    academic_term_id uuid,
    name text NOT NULL,
    holiday_date date NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone,
    deleted_at timestamp with time zone,
    created_by uuid,
    updated_by uuid,
    CONSTRAINT "PK_holidays" PRIMARY KEY (id),
    CONSTRAINT "FK_holidays_academic_terms_academic_term_id" FOREIGN KEY (academic_term_id) REFERENCES academic_terms (id),
    CONSTRAINT "FK_holidays_institutions_institution_id" FOREIGN KEY (institution_id) REFERENCES institutions (id) ON DELETE CASCADE
);

CREATE TABLE programs (
    id uuid NOT NULL,
    "InstitutionId" uuid NOT NULL,
    department_id uuid NOT NULL,
    name text NOT NULL,
    is_active boolean NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone,
    deleted_at timestamp with time zone,
    created_by uuid,
    updated_by uuid,
    CONSTRAINT "PK_programs" PRIMARY KEY (id),
    CONSTRAINT "FK_programs_departments_department_id" FOREIGN KEY (department_id) REFERENCES departments (id) ON DELETE CASCADE
);

CREATE TABLE parent_student_links (
    id uuid NOT NULL,
    parent_id uuid NOT NULL,
    student_id uuid NOT NULL,
    created_at timestamp with time zone NOT NULL,
    deleted_at timestamp with time zone,
    created_by uuid,
    CONSTRAINT "PK_parent_student_links" PRIMARY KEY (id),
    CONSTRAINT "FK_parent_student_links_parents_parent_id" FOREIGN KEY (parent_id) REFERENCES parents (id) ON DELETE CASCADE,
    CONSTRAINT "FK_parent_student_links_students_student_id" FOREIGN KEY (student_id) REFERENCES students (id) ON DELETE CASCADE
);

CREATE TABLE courses (
    id uuid NOT NULL,
    "InstitutionId" uuid NOT NULL,
    program_id uuid NOT NULL,
    name text NOT NULL,
    code text,
    is_active boolean NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone,
    deleted_at timestamp with time zone,
    created_by uuid,
    updated_by uuid,
    CONSTRAINT "PK_courses" PRIMARY KEY (id),
    CONSTRAINT "FK_courses_programs_program_id" FOREIGN KEY (program_id) REFERENCES programs (id) ON DELETE CASCADE
);

CREATE TABLE classes (
    id uuid NOT NULL,
    "InstitutionId" uuid NOT NULL,
    course_id uuid NOT NULL,
    academic_term_id uuid NOT NULL,
    name text NOT NULL,
    is_active boolean NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone,
    deleted_at timestamp with time zone,
    created_by uuid,
    updated_by uuid,
    CONSTRAINT "PK_classes" PRIMARY KEY (id),
    CONSTRAINT "FK_classes_academic_terms_academic_term_id" FOREIGN KEY (academic_term_id) REFERENCES academic_terms (id) ON DELETE CASCADE,
    CONSTRAINT "FK_classes_courses_course_id" FOREIGN KEY (course_id) REFERENCES courses (id) ON DELETE CASCADE
);

CREATE TABLE activities (
    id uuid NOT NULL,
    class_id uuid NOT NULL,
    institution_id uuid NOT NULL,
    activity_type text NOT NULL,
    title text NOT NULL,
    description text,
    scheduled_date date NOT NULL,
    scheduled_time time,
    estimated_weight numeric(5,2),
    status text NOT NULL,
    metadata jsonb,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone,
    deleted_at timestamp with time zone,
    created_by uuid,
    updated_by uuid,
    CONSTRAINT "PK_activities" PRIMARY KEY (id),
    CONSTRAINT "FK_activities_classes_class_id" FOREIGN KEY (class_id) REFERENCES classes (id) ON DELETE CASCADE
);

CREATE TABLE class_students (
    id uuid NOT NULL,
    class_id uuid NOT NULL,
    student_id uuid NOT NULL,
    created_at timestamp with time zone NOT NULL,
    deleted_at timestamp with time zone,
    created_by uuid,
    CONSTRAINT "PK_class_students" PRIMARY KEY (id),
    CONSTRAINT "FK_class_students_classes_class_id" FOREIGN KEY (class_id) REFERENCES classes (id) ON DELETE CASCADE,
    CONSTRAINT "FK_class_students_students_student_id" FOREIGN KEY (student_id) REFERENCES students (id) ON DELETE CASCADE
);

CREATE TABLE class_teachers (
    id uuid NOT NULL,
    class_id uuid NOT NULL,
    teacher_id uuid NOT NULL,
    created_at timestamp with time zone NOT NULL,
    deleted_at timestamp with time zone,
    created_by uuid,
    CONSTRAINT "PK_class_teachers" PRIMARY KEY (id),
    CONSTRAINT "FK_class_teachers_classes_class_id" FOREIGN KEY (class_id) REFERENCES classes (id) ON DELETE CASCADE,
    CONSTRAINT "FK_class_teachers_teachers_teacher_id" FOREIGN KEY (teacher_id) REFERENCES teachers (id) ON DELETE CASCADE
);

CREATE TABLE notifications (
    id uuid NOT NULL,
    recipient_id uuid NOT NULL,
    institution_id uuid NOT NULL,
    category text NOT NULL,
    title text NOT NULL,
    body text NOT NULL,
    related_activity_id uuid,
    is_read boolean NOT NULL,
    sent_via_push boolean NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone,
    deleted_at timestamp with time zone,
    created_by uuid,
    updated_by uuid,
    CONSTRAINT "PK_notifications" PRIMARY KEY (id),
    CONSTRAINT "FK_notifications_activities_related_activity_id" FOREIGN KEY (related_activity_id) REFERENCES activities (id),
    CONSTRAINT "FK_notifications_profiles_recipient_id" FOREIGN KEY (recipient_id) REFERENCES profiles (id) ON DELETE CASCADE
);

CREATE TABLE rule_evaluation_logs (
    id uuid NOT NULL,
    activity_id uuid,
    institution_id uuid NOT NULL,
    outcome text NOT NULL,
    triggered_rule_id uuid,
    reason_code text,
    override_note text,
    evaluated_by uuid,
    evaluated_at timestamp with time zone NOT NULL,
    CONSTRAINT "PK_rule_evaluation_logs" PRIMARY KEY (id),
    CONSTRAINT "FK_rule_evaluation_logs_activities_activity_id" FOREIGN KEY (activity_id) REFERENCES activities (id),
    CONSTRAINT "FK_rule_evaluation_logs_rule_definitions_triggered_rule_id" FOREIGN KEY (triggered_rule_id) REFERENCES rule_definitions (id)
);

CREATE INDEX ix_academic_terms_institution_id ON academic_terms (institution_id);

CREATE INDEX ix_activities_activity_type ON activities (activity_type);

CREATE INDEX ix_activities_class_id ON activities (class_id);

CREATE INDEX ix_activities_institution_id ON activities (institution_id);

CREATE INDEX ix_activities_scheduled_date ON activities (scheduled_date);

CREATE INDEX ix_class_students_student_id ON class_students (student_id);

CREATE UNIQUE INDEX uq_class_students_class_student ON class_students (class_id, student_id) WHERE deleted_at IS NULL;

CREATE INDEX ix_class_teachers_teacher_id ON class_teachers (teacher_id);

CREATE UNIQUE INDEX uq_class_teachers_class_teacher ON class_teachers (class_id, teacher_id) WHERE deleted_at IS NULL;

CREATE INDEX ix_classes_academic_term_id ON classes (academic_term_id);

CREATE INDEX ix_classes_course_id ON classes (course_id);

CREATE INDEX ix_courses_program_id ON courses (program_id);

CREATE INDEX ix_departments_institution_id ON departments (institution_id);

CREATE INDEX ix_device_tokens_profile_id ON device_tokens (profile_id);

CREATE UNIQUE INDEX uq_device_tokens_token ON device_tokens (token);

CREATE INDEX "IX_holidays_academic_term_id" ON holidays (academic_term_id);

CREATE INDEX ix_holidays_holiday_date ON holidays (holiday_date);

CREATE INDEX ix_holidays_institution_id ON holidays (institution_id);

CREATE INDEX ix_notifications_is_read ON notifications (is_read);

CREATE INDEX ix_notifications_recipient_id ON notifications (recipient_id);

CREATE INDEX "IX_notifications_related_activity_id" ON notifications (related_activity_id);

CREATE INDEX "IX_parent_student_links_student_id" ON parent_student_links (student_id);

CREATE UNIQUE INDEX uq_parent_student_links_parent_student ON parent_student_links (parent_id, student_id) WHERE deleted_at IS NULL;

CREATE UNIQUE INDEX uq_parents_profile_id ON parents (profile_id);

CREATE INDEX ix_profiles_institution_id ON profiles (institution_id);

CREATE INDEX ix_profiles_role ON profiles (role);

CREATE UNIQUE INDEX uq_profiles_email ON profiles (email);

CREATE INDEX ix_programs_department_id ON programs (department_id);

CREATE INDEX ix_rule_definitions_institution_id ON rule_definitions (institution_id);

CREATE INDEX ix_rule_definitions_rule_type ON rule_definitions (rule_type);

CREATE INDEX ix_rule_definitions_scope ON rule_definitions (scope_level, scope_id);

CREATE INDEX ix_rule_evaluation_logs_activity_id ON rule_evaluation_logs (activity_id);

CREATE INDEX ix_rule_evaluation_logs_evaluated_at ON rule_evaluation_logs (evaluated_at);

CREATE INDEX ix_rule_evaluation_logs_institution_id ON rule_evaluation_logs (institution_id);

CREATE INDEX "IX_rule_evaluation_logs_triggered_rule_id" ON rule_evaluation_logs (triggered_rule_id);

CREATE INDEX ix_students_institution_id ON students (institution_id);

CREATE UNIQUE INDEX uq_students_profile_id ON students (profile_id);

CREATE INDEX ix_teachers_institution_id ON teachers (institution_id);

CREATE UNIQUE INDEX uq_teachers_profile_id ON teachers (profile_id);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260730225319_InitialCreate', '9.0.0');

ALTER TABLE teachers ADD department_id uuid;

CREATE INDEX ix_teachers_department_id ON teachers (department_id);

ALTER TABLE teachers ADD CONSTRAINT "FK_teachers_departments_department_id" FOREIGN KEY (department_id) REFERENCES departments (id);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260731020320_AddTeacherDepartmentId', '9.0.0');

ALTER TABLE activities ADD duration interval;

ALTER TABLE activities ADD end_time time;

ALTER TABLE activities ADD priority integer NOT NULL DEFAULT 1;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260803020038_PhaseG1FinalSync', '9.0.0');

COMMIT;

