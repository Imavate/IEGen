using AutoMapper;
using IEGen.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Data.Entity;

namespace IEGen
{
    public class AutoMapperConfig
    {
        public static void RegisterMappings()
        {
            Mapper.Initialize(m =>
            {
                // Setup
                m.CreateMap<SchoolReg, SchoolRegViewModel>();
                m.CreateMap<SchoolReg, NewRequestViewModel>().ForMember(d => d.RequestDate, s => s.MapFrom(x => x.RegDate));
                m.CreateMap<AddSchoolRegViewModel, SchoolReg>();

                m.CreateMap<SchoolReg, EditSchoolViewModel>();

                m.CreateMap<ED_STUDENT, FormerStudentMiniModel>().ForMember(d => d.SchoolName, s => s.MapFrom(x => x.INSTITUTION.INSTITUTION_NAME))
                                                                 .ForMember(d => d.ClassName, s => s.MapFrom(x => x.ED_CLASS1.CLASS_NAME))
                                                                 .ForMember(d => d.TermName, s => s.MapFrom(x => x.ED_CLASS1.ED_ACADEMIC_TERM.TERM_CAPTION));

                m.CreateMap<ED_STUDENT, FormerStudentViewModel>().IncludeBase<ED_STUDENT, FormerStudentMiniModel>();

                m.CreateMap<ED_STUDENT, FormerStudentDisplayModel>().IncludeBase<ED_STUDENT, FormerStudentMiniModel>()
                                                                    .ForMember(d => d.Picture, s => s.MapFrom(x => x.FILE_IMAGE.CONTENT));

                m.CreateMap<ED_STUDENT_COURSE, FormerResultModel>().ForMember(d => d.TermName, s => s.MapFrom(x => x.ED_ACADEMIC_TERM.TERM_CAPTION))
                                                                   .ForMember(d => d.TermNumber, s => s.MapFrom(x => x.ED_ACADEMIC_TERM.TERM_NUMBER.Value))
                                                                   .ForMember(d => d.Level, s => s.MapFrom(x => x.ED_COURSE.ACADEMIC_LEVEL_ID))
                                                                   .ForMember(d => d.CourseName, s => s.MapFrom(x => x.ED_COURSE.COURSE_NAME))
                                                                   .ForMember(d => d.DefinitionName, s => s.MapFrom(x => x.ED_COURSE.ED_COURSE_DEFINITION.DEFINITION_NAME))
                                                                   .ForMember(d => d.Score, s => s.MapFrom(x => x.PERCENT_SCORE));


                m.CreateMap<School, ChooseSchoolViewModel>().ForMember(d => d.StateID, s => s.MapFrom(x => x.Location.StateID))
                                                            .ForMember(d => d.LocationName, s => s.MapFrom(x => x.Location.Name));

                m.CreateMap<School, SchoolViewModel>().ForMember(d => d.TermCount, s => s.MapFrom(x => x.Terms.Count()))
                                                      .ForMember(d => d.StateID, s => s.MapFrom(x => x.Location.StateID))
                                                      .ForMember(d => d.LocationName, s => s.MapFrom(x => x.Location.Name));

                m.CreateMap<School, EditSchoolViewModel>().ForMember(d => d.HasTerms, s => s.MapFrom(x => x.Terms.Any()));
                m.CreateMap<EditSchoolViewModel, School>();

                m.CreateMap<AccessGroup, AccessGroupViewModel>().ForMember(d => d.ChangedByName, s => s.MapFrom(x => x.ChangedBy.Name))
                                                                .ForMember(d => d.UserCount, s => s.MapFrom(x => x.Users.Count()))
                                                                .ForMember(d => d.RoleCount, s => s.MapFrom(x => x.GroupRoles.Count()));

                m.CreateMap<AccessGroupRole, GroupRoleViewModel>().ForMember(d => d.Name, s => s.MapFrom(x => x.Role.Name));
                m.CreateMap<AccessGroup, EditGroupRolesViewModel>().ForMember(d => d.UserCount, s => s.MapFrom(x => x.Users.Count()))
                                                                   .ForMember(d => d.AssignedRoles, s => s.MapFrom(x => x.GroupRoles));

                m.CreateMap<Location, LocationViewModel>().ForMember(d => d.SchoolCount, s => s.MapFrom(x => x.Schools.Count()));

                m.CreateMap<EditUserViewModel, IEUser>();
                m.CreateMap<IEUser, EditUserViewModel>();

                m.CreateMap<IEUser, UserViewModel>().ForMember(d => d.AccessGroupName, s => s.MapFrom(x => x.AccessGroup.Name));
                m.CreateMap<IEUser, UserMiniViewModel>();
                m.CreateMap<IEUser, UserDetailViewModel>();


                m.CreateMap<Grade, GradeViewModel>();
                m.CreateMap<Grade, EditGradeViewModel>();
                m.CreateMap<EditGradeViewModel, Grade>();
                m.CreateMap<GradeGroup, GradeGroupViewModel>().ForMember(d => d.SchoolCount, s => s.MapFrom(x => x.Schools.Count()))
                                                              .ForMember(d => d.GradeCount, s => s.MapFrom(x => x.Grades.Count()))
                                                              .ForMember(d => d.IsUsed, s => s.MapFrom(x => x.Classes.Any()));
                m.CreateMap<GradeGroup, GradeSetupViewModel>().ForMember(d => d.SchoolCount, s => s.MapFrom(x => x.Schools.Count()))
                                                              .ForMember(d => d.GradeList, s => s.MapFrom(x => x.Grades));
                m.CreateMap<GradeGroup, ViewGradeGroupViewModel>().ForMember(d => d.GradeList, s => s.MapFrom(x => x.Grades))
                                                                  .ForMember(d => d.IsUsed, s => s.MapFrom(x => x.Classes.Any()));

                m.CreateMap<SkillGrade, SkillGradeViewModel>();
                m.CreateMap<SkillGradeViewModel, SkillGrade>();
                m.CreateMap<SkillGroupViewModel, SkillGroup>();
                m.CreateMap<SkillGroup, SkillGroupViewModel>().ForMember(d => d.SchoolCount, s => s.MapFrom(x => x.Schools.Count()))
                                                              .ForMember(d => d.GradeCount, s => s.MapFrom(x => x.Grades.Count()))
                                                              .ForMember(d => d.IsUsed, s => s.MapFrom(x => x.Classes.Any()));
                m.CreateMap<SkillGroup, SkillSetupViewModel>().ForMember(d => d.SchoolCount, s => s.MapFrom(x => x.Schools.Count()))
                                                              .ForMember(d => d.GradeList, s => s.MapFrom(x => x.Grades));
                m.CreateMap<SkillGroup, ViewSkillGroupViewModel>().ForMember(d => d.GradeList, s => s.MapFrom(x => x.Grades))
                                                                  .ForMember(d => d.IsUsed, s => s.MapFrom(x => x.Classes.Any()));

                m.CreateMap<Teacher, TeacherViewModel>().ForMember(d => d.Qualifications, s => s.MapFrom(x => x.CTeacher.Qualifications))
                                                        .ForMember(d => d.StartYear, s => s.MapFrom(x => x.CTeacher.StartYear))
                                                        .ForMember(d => d.SchoolName, s => s.MapFrom(x => x.School.Name))
                                                        .ForMember(d => d.StateID, s => s.MapFrom(x => x.School.Location.StateID))
                                                        .ForMember(d => d.LocationName, s => s.MapFrom(x => x.School.Location.Name));

                m.CreateMap<TeacherViewModel, Teacher>();

                m.CreateMap<CTeacher, CTeacherViewModel>().ForMember(d => d.StateID, s => s.MapFrom(x => x.Location.StateID))
                                                          .ForMember(d => d.LocationName, s => s.MapFrom(x => x.Location.Name));

                m.CreateMap<CTeacher, EditCTeacherViewModel>().ForMember(d => d.OldEmail, s => s.MapFrom(x => x.Email));
                m.CreateMap<EditCTeacherViewModel, CTeacher>();
                m.CreateMap<EditCTeacherViewModel, IEUser>().ForMember(d => d.PhoneNumber, s => s.MapFrom(x => x.Phone));

                m.CreateMap<Term, TermViewModel>().ForMember(d => d.ClassCount, s => s.MapFrom(x => x.Classes.Count()))
                                                  .ForMember(d => d.StudentCount, s => s.MapFrom(x => x.Classes.Sum(c => c.Students.Count())))
                                                  .ForMember(d => d.ResultCount, s => s.MapFrom(x => x.Classes.Sum(c => c.Results.Count())));
                m.CreateMap<Term, EditTermViewModel>().ForMember(d => d.ClassCount, s => s.MapFrom(x => x.Classes.Count()));
                m.CreateMap<EditTermViewModel, Term>();

                m.CreateMap<School, SchoolDetailsViewModel>().ForMember(d => d.LocationName, s => s.MapFrom(x => x.Location.Name));
                m.CreateMap<School, SchoolRequestPageViewModel>().ForMember(d => d.LocationName, s => s.MapFrom(x => x.Location.Name));


                m.CreateMap<School, SchoolGradesViewModel>().ForMember(d => d.LocationName, s => s.MapFrom(x => x.Location.Name));
                m.CreateMap<SchoolData, SchoolGradesViewModel>().ForMember(d => d.Name, s => s.MapFrom(x => x.School.Name))
                                                                .ForMember(d => d.LocationName, s => s.MapFrom(x => x.School.Location.Name))
                                                                .ForMember(d => d.TypeID, s => s.MapFrom(x => x.School.TypeID))
                                                                .ForMember(d => d.DefGroupName, s => s.MapFrom(x => x.DefaultGradeGroup.Name));

                m.CreateMap<School, SchoolSkillsViewModel>().ForMember(d => d.LocationName, s => s.MapFrom(x => x.Location.Name));
                m.CreateMap<SchoolData, SchoolSkillsViewModel>().ForMember(d => d.Name, s => s.MapFrom(x => x.School.Name))
                                                                .ForMember(d => d.LocationName, s => s.MapFrom(x => x.School.Location.Name))
                                                                .ForMember(d => d.TypeID, s => s.MapFrom(x => x.School.TypeID))
                                                                .ForMember(d => d.DefGroupName, s => s.MapFrom(x => x.DefaultSkillGroup.Name));


                m.CreateMap<School, SchoolPerformanceCommentViewModel>().ForMember(d => d.LocationName, s => s.MapFrom(x => x.Location.Name));
                m.CreateMap<SchoolData, SchoolPerformanceCommentViewModel>().ForMember(d => d.Name, s => s.MapFrom(x => x.School.Name))
                                                                            .ForMember(d => d.LocationName, s => s.MapFrom(x => x.School.Location.Name))
                                                                            .ForMember(d => d.TypeID, s => s.MapFrom(x => x.School.TypeID))
                                                                            .ForMember(d => d.DefGroupName, s => s.MapFrom(x => x.DefPerformanceCommentGroup.Name));

                m.CreateMap<PerformanceComment, PerformanceCommentViewModel>();
                m.CreateMap<PerformanceCommentViewModel, PerformanceComment>();
                m.CreateMap<PerformanceCommentGroupViewModel, PerformanceCommentGroup>();
                m.CreateMap<PerformanceCommentGroup, PerformanceCommentGroupViewModel>().ForMember(d => d.CommentCount, s => s.MapFrom(x => x.Comments.Count()))
                                                                                        .ForMember(d => d.ClassCount, s => s.MapFrom(x => x.Classes.Count()));
                m.CreateMap<PerformanceCommentGroup, PerformanceCommentSetupViewModel>().ForMember(d => d.SchoolName, s => s.MapFrom(x => x.School.Name))
                                                                                        .ForMember(d => d.ClassCount, s => s.MapFrom(x => x.Classes.Count()))
                                                                                        .ForMember(d => d.CommentList, s => s.MapFrom(x => x.Comments));


                m.CreateMap<School, SchoolPromotionCommentViewModel>().ForMember(d => d.LocationName, s => s.MapFrom(x => x.Location.Name));
                m.CreateMap<SchoolData, SchoolPromotionCommentViewModel>().ForMember(d => d.Name, s => s.MapFrom(x => x.School.Name))
                                                                            .ForMember(d => d.LocationName, s => s.MapFrom(x => x.School.Location.Name))
                                                                            .ForMember(d => d.TypeID, s => s.MapFrom(x => x.School.TypeID))
                                                                            .ForMember(d => d.DefGroupName, s => s.MapFrom(x => x.DefPromotionCommentGroup.Name));

                m.CreateMap<PromotionComment, PromotionCommentViewModel>();
                m.CreateMap<PromotionCommentViewModel, PromotionComment>();
                m.CreateMap<PromotionCommentGroupViewModel, PromotionCommentGroup>();
                m.CreateMap<PromotionCommentGroup, PromotionCommentGroupViewModel>().ForMember(d => d.CommentCount, s => s.MapFrom(x => x.Comments.Count()))
                                                                                        .ForMember(d => d.ClassCount, s => s.MapFrom(x => x.Classes.Count()));
                m.CreateMap<PromotionCommentGroup, PromotionCommentSetupViewModel>().ForMember(d => d.SchoolName, s => s.MapFrom(x => x.School.Name))
                                                                                    .ForMember(d => d.ClassCount, s => s.MapFrom(x => x.Classes.Count()))
                                                                                    .ForMember(d => d.CommentList, s => s.MapFrom(x => x.Comments));


                m.CreateMap<School, SchoolImprovementCommentViewModel>().ForMember(d => d.LocationName, s => s.MapFrom(x => x.Location.Name));
                m.CreateMap<SchoolData, SchoolImprovementCommentViewModel>().ForMember(d => d.Name, s => s.MapFrom(x => x.School.Name))
                                                                            .ForMember(d => d.LocationName, s => s.MapFrom(x => x.School.Location.Name))
                                                                            .ForMember(d => d.TypeID, s => s.MapFrom(x => x.School.TypeID))
                                                                            .ForMember(d => d.DefGroupName, s => s.MapFrom(x => x.DefImprovementCommentGroup.Name));

                m.CreateMap<ImprovementComment, ImprovementCommentViewModel>();
                m.CreateMap<ImprovementCommentViewModel, ImprovementComment>();
                m.CreateMap<ImprovementCommentGroupViewModel, ImprovementCommentGroup>();
                m.CreateMap<ImprovementCommentGroup, ImprovementCommentGroupViewModel>().ForMember(d => d.CommentCount, s => s.MapFrom(x => x.Comments.Count()))
                                                                                        .ForMember(d => d.ClassCount, s => s.MapFrom(x => x.Classes.Count()));
                m.CreateMap<ImprovementCommentGroup, ImprovementCommentSetupViewModel>().ForMember(d => d.SchoolName, s => s.MapFrom(x => x.School.Name))
                                                                                        .ForMember(d => d.ClassCount, s => s.MapFrom(x => x.Classes.Count()))
                                                                                        .ForMember(d => d.CommentList, s => s.MapFrom(x => x.Comments));

                m.CreateMap<SchoolData, SchoolDefaultsModel>().ForMember(d => d.DefImprovementCommentGroup, s => s.MapFrom(x => x.DefImprovementCommentGroup.Name))
                                                              .ForMember(d => d.DefGradeGroup, s => s.MapFrom(x => x.DefaultGradeGroup.Name))
                                                              .ForMember(d => d.DefSkillGroup, s => s.MapFrom(x => x.DefaultSkillGroup.Name))
                                                              .ForMember(d => d.DefPerformanceCommentGroup, s => s.MapFrom(x => x.DefPerformanceCommentGroup.Name));


                m.CreateMap<School, ClassArmPageViewModel>().ForMember(d => d.SchoolName, s => s.MapFrom(x => x.Name))
                                                            .ForMember(d => d.LocationName, s => s.MapFrom(x => x.Location.Name))
                                                            .ForMember(d => d.SchoolTypeID, s => s.MapFrom(x => x.TypeID));

                m.CreateMap<EditClassArmViewModel, ClassArm>();
                m.CreateMap<ClassArm, EditClassArmViewModel>().ForMember(d => d.HasClasses, s => s.MapFrom(x => x.Classes.Any()))
                                                              .ForMember(d => d.SchoolTypeID, s => s.MapFrom(x => x.School.TypeID));
                m.CreateMap<EditClassArmViewModel, ClassArmViewModel>();
                m.CreateMap<ClassArm, ClassArmViewModel>().ForMember(d => d.HasClasses, s => s.MapFrom(x => x.Classes.Any()));

                m.CreateMap<Term, ClassPageViewModel>().ForMember(d => d.ClassList, s => s.MapFrom(x => x.Classes))
                                                       .ForMember(d => d.SchoolTypeID, s => s.MapFrom(x => x.School.TypeID));

                m.CreateMap<Class, ClassDataModel>().ForMember(d => d.Name, s => s.MapFrom(x => x.Arm.Name))
                                                    .ForMember(d => d.StudentCount, s => s.MapFrom(x => x.Students.Count()))
                                                    .ForMember(d => d.SubjectCount, s => s.MapFrom(x => x.Subjects.Count()));

                m.CreateMap<Class, ClassMiniModel>().ForMember(d => d.Name, s => s.MapFrom(x => x.Arm.Name))
                                                    .ForMember(d => d.ClassLevelID, s => s.MapFrom(x => x.Arm.ClassLevelID))
                                                    .ForMember(d => d.ClassTypeID, s => s.MapFrom(x => x.Arm.ClassTypeID))
                                                    .ForMember(d => d.StudentCount, s => s.MapFrom(x => x.Students.Count()));

                m.CreateMap<Class, ClassViewModel>().IncludeBase<Class, ClassMiniModel>()
                                                    .ForMember(d => d.SubjectCount, s => s.MapFrom(x => x.Subjects.Count()))
                                                    .ForMember(d => d.GradeGroupName, s => s.MapFrom(x => x.GradeGroup.Name));

                m.CreateMap<Term, ImportStudentsPageViewModel>().ForMember(d => d.StudentCount, s => s.MapFrom(x => x.Classes.Sum(c => c.Students.Count())));
                m.CreateMap<Term, ImportTermStudentsViewModel>();

                m.CreateMap<Class, ImportClassStudentsViewModel>().ForMember(d => d.Name, s => s.MapFrom(x => x.Arm.Name))
                                                                  .ForMember(d => d.ClassTypeID, s => s.MapFrom(x => x.Arm.ClassTypeID))
                                                                  .ForMember(d => d.StudentCount, s => s.MapFrom(x => x.Students.Count()));

                m.CreateMap<Class, ImportClassViewModel>();
                m.CreateMap<ImportClassViewModel, Class>().ForMember(d => d.ClassID, s => s.Ignore())
                                                          .ForMember(d => d.Subjects, s => s.Ignore());
                m.CreateMap<EditClassViewModel, Class>();
                m.CreateMap<Class, EditClassViewModel>().ForMember(d => d.Name, s => s.MapFrom(x => x.Arm.Name))
                                                        .ForMember(d => d.StudentCount, s => s.MapFrom(x => x.Students.Count()))
                                                        .ForMember(d => d.SubjectCount, s => s.MapFrom(x => x.Subjects.Count()));

                m.CreateMap<Class, ClassSetupViewModel>().ForMember(d => d.Name, s => s.MapFrom(x => x.Arm.Name))
                                                         .ForMember(d => d.ClassLevelID, s => s.MapFrom(x => x.Arm.ClassLevelID))
                                                         .ForMember(d => d.ClassTypeID, s => s.MapFrom(x => x.Arm.ClassTypeID))
                                                         .ForMember(d => d.StudentCount, s => s.MapFrom(x => x.Students.Count()))
                                                         .ForMember(d => d.TermName, s => s.MapFrom(x => x.Term.Name))
                                                         .ForMember(d => d.TermNumber, s => s.MapFrom(x => x.Term.TermNumber))
                                                         .ForMember(d => d.SchoolYear, s => s.MapFrom(x => x.Term.SchoolYear))
                                                         .ForMember(d => d.SubjectList, s => s.MapFrom(x => x.Subjects));

                m.CreateMap<ComplexScoreFormat, ComplexResultFormatViewModel>();
                m.CreateMap<EditComplexResultFormatViewModel, ComplexScoreFormat>();
                m.CreateMap<ComplexScoreFormat, EditComplexResultFormatViewModel>().ForMember(d => d.ClassCount, s => s.MapFrom(x => x.Classes.Count()))
                                                                                   .ForMember(d => d.SubjectCount, s => s.MapFrom(x => x.Subjects.Count()));

                m.CreateMap<SubjectTemplate, Subject>();
                m.CreateMap<Subject, SubjectViewModel>().ForMember(d => d.Order, s => s.MapFrom(x => x.Template.Order))
                                                        .ForMember(d => d.Name, s => s.MapFrom(x => x.Template.Name))
                                                        .ForMember(d => d.CategoryID, s => s.MapFrom(x => x.Template.CategoryID))
                                                        .ForMember(d => d.ClassLevelID, s => s.MapFrom(x => x.Template.ClassLevelID))
                                                        .ForMember(d => d.TeacherName, s => s.MapFrom(x => x.Teacher.Name));
                m.CreateMap<Subject, SubjectMiniModel>().ForMember(d => d.Name, s => s.MapFrom(x => x.Template.Name));
                m.CreateMap<SubjectTemplate, SubjectTemplateMiniModel>();
                m.CreateMap<SubjectTemplate, SubjectTemplateViewModel>().ForMember(d => d.SubjectID, s => s.Ignore());
                m.CreateMap<SubjectTemplate, EditSubjectTemplateViewModel>();
                m.CreateMap<EditSubjectTemplateViewModel, SubjectTemplate>();

                m.CreateMap<EditSubjectViewModel, Subject>();
                m.CreateMap<Subject, EditSubjectViewModel>().ForMember(d => d.Name, s => s.MapFrom(x => x.Template.Name))
                                                            .ForMember(d => d.ClassName, s => s.MapFrom(x => x.Class.Arm.Name))
                                                            .ForMember(d => d.CategoryID, s => s.MapFrom(x => x.Template.CategoryID))
                                                            .ForMember(d => d.ClassLevelID, s => s.MapFrom(x => x.Template.ClassLevelID))
                                                            .ForMember(d => d.HasResults, s => s.MapFrom(x => x.EnteredByID.HasValue));

                m.CreateMap<Subject, SubjectDownloadModel>().IncludeBase<Subject, SubjectViewModel>()
                                                            .ForMember(d => d.Name, s => s.MapFrom(x => x.Template.Name))
                                                            .ForMember(d => d.ClassName, s => s.MapFrom(x => x.Class.Arm.Name))
                                                            .ForMember(d => d.EnteredByName, s => s.MapFrom(x => x.EnteredBy.Name))
                                                            .ForMember(d => d.VerifiedByName, s => s.MapFrom(x => x.VerifiedBy.Name));

                m.CreateMap<Subject, SubjectEntryViewModel>().ForMember(d => d.Name, s => s.MapFrom(x => x.Template.Name))
                                                            .ForMember(d => d.ClassName, s => s.MapFrom(x => x.Class.Arm.Name))
                                                            .ForMember(d => d.TeacherName, s => s.MapFrom(x => x.Teacher.Name))
                                                            .ForMember(d => d.EnteredByName, s => s.MapFrom(x => x.EnteredBy.Name))
                                                            .ForMember(d => d.VerifiedByName, s => s.MapFrom(x => x.VerifiedBy.Name));

                m.CreateMap<Student, AllStudentViewModel>().ForMember(d => d.ClassName, s => s.MapFrom(x => x.Class.Arm.Name))
                                                           .ForMember(d => d.TermName, s => s.MapFrom(x => x.Class.Term.Name))
                                                           .ForMember(d => d.SchoolName, s => s.MapFrom(x => x.School.Name))
                                                           .ForMember(d => d.SchoolLocation, s => s.MapFrom(x => x.School.Location.Name))
                                                           .ForMember(d => d.SchoolStateID, s => s.MapFrom(x => x.School.Location.StateID))
                                                           .ForMember(d => d.LocationName, s => s.MapFrom(x => x.Location.Name))
                                                           .ForMember(d => d.StateID, s => s.MapFrom(x => x.Location.StateID));

                m.CreateMap<Student, StudentMiniViewModel>().ForMember(d => d.ClassName, s => s.MapFrom(x => x.Class.Arm.Name))
                                                            .ForMember(d => d.TermName, s => s.MapFrom(x => x.Class.Term.Name));

                m.CreateMap<Student, StudentViewModel>().IncludeBase<Student, StudentMiniViewModel>()
                                                        .ForMember(d => d.ClassLevelID, s => s.MapFrom(x => x.Class.Arm.ClassLevelID));

                m.CreateMap<EditStudentViewModel, Student>();
                m.CreateMap<EditFloatingStudentViewModel, Student>();
                m.CreateMap<Student, EditStudentViewModel>().ForMember(d => d.TermID, s => s.MapFrom(x => x.Class.TermID))
                                                            .ForMember(d => d.OldClassID, s => s.MapFrom(x => x.ClassID))
                                                            .ForMember(d => d.TermName, s => s.MapFrom(x => x.Class.Term.Name))
                                                            .ForMember(d => d.ClassName, s => s.MapFrom(x => x.Class.Arm.Name))
                                                            .ForMember(d => d.HasComplexScores, s => s.MapFrom(x => x.ComplexScores.Any()))
                                                            .ForMember(d => d.HasScores, s => s.MapFrom(x => x.Scores.Any()));
                m.CreateMap<Student, EditFloatingStudentViewModel>().ForMember(d => d.HasComplexScores, s => s.MapFrom(x => x.ComplexScores.Any()))
                                                                    .ForMember(d => d.HasScores, s => s.MapFrom(x => x.Scores.Any()));

                m.CreateMap<Student, StudentPageViewModel>().ForMember(d => d.ClassName, s => s.MapFrom(x => x.Class.Arm.Name))
                                                            .ForMember(d => d.HasClass, s => s.MapFrom(x => x.ClassID.HasValue))
                                                            .ForMember(d => d.LocationName, s => s.MapFrom(x => x.Location.Name))
                                                            .ForMember(d => d.StateID, s => s.MapFrom(x => x.Location.StateID));

                m.CreateMap<Student, UploadStudentScoreViewModel>().IncludeBase<Student, StudentMiniViewModel>();
                m.CreateMap<ResultCsvModel, ResultUploadModel>();
                m.CreateMap<UploadStudentScoreViewModel, VerifyStudentScoreUploadViewModel>();

                m.CreateMap<StudentPastResult, StudentPastResultViewModel>().ForMember(d => d.SubjectName, s => s.MapFrom(x => x.Subject.Name))
                                                                            .ForMember(d => d.CategoryID, s => s.MapFrom(x => x.Subject.CategoryID))
                                                                            .ForMember(d => d.ClassLevelID, s => s.MapFrom(x => x.Subject.ClassLevelID));
                m.CreateMap<Student, FloatingStudentViewModel>();

                m.CreateMap<ScoreEntry, CurrentResultModel>().ForMember(d => d.SubjectName, s => s.MapFrom(x => x.Subject.Template.Name))
                                                             .ForMember(d => d.Year, s => s.MapFrom(x => x.Subject.SchoolYear))
                                                             .ForMember(d => d.TermNumber, s => s.MapFrom(x => x.Subject.TermNumber))
                                                             .ForMember(d => d.ClassLevelID, s => s.MapFrom(x => x.Subject.Template.ClassLevelID));


                m.CreateMap<School, FloatingStudentsPageViewModel>().ForMember(d => d.SchoolName, s => s.MapFrom(x => x.Name))
                                                                    .ForMember(d => d.LocationName, s => s.MapFrom(x => x.Location.Name))
                                                                    .ForMember(d => d.SchoolTypeID, s => s.MapFrom(x => x.TypeID));


                m.CreateMap<ScoreEntryViewModel, ScoreEntry>();
                m.CreateMap<ScoreEntry, ScoreEntryMiniViewModel>().ForMember(d => d.FirstName, s => s.MapFrom(x => x.Student.FirstName))
                                                                  .ForMember(d => d.Surname, s => s.MapFrom(x => x.Student.Surname));
                m.CreateMap<ScoreEntry, ScoreEntryViewModel>().IncludeBase<ScoreEntry, ScoreEntryMiniViewModel>();

                m.CreateMap<Subject, ScorePageViewModel>().ForMember(d => d.Name, s => s.MapFrom(x => x.Template.Name))
                                                          .ForMember(d => d.ClassName, s => s.MapFrom(x => x.Class.Arm.Name))
                                                          .ForMember(d => d.TeacherName, s => s.MapFrom(x => x.Teacher.Name))
                                                          .ForMember(d => d.MaxCAScore, s => s.MapFrom(x => x.Class.CAWeight))
                                                          .ForMember(d => d.MaxExamScore, s => s.MapFrom(x => x.Class.ExamWeight))
                                                          .ForMember(d => d.Scores, s => s.MapFrom(x => x.Scores.OrderBy(l => l.SerialNumber)));

                m.CreateMap<ScoreEntry, ScoreVerifyViewModel>().IncludeBase<ScoreEntry, ScoreEntryMiniViewModel>();
                m.CreateMap<Subject, VerifyPageViewModel>().IncludeBase<Subject, ScorePageViewModel>()
                                                           .ForMember(d => d.GradeGroupID, s => s.MapFrom(x => x.Class.GradeGroupID));

                m.CreateMap<ScoreVerifyViewModel, ScoreModifyViewModel>();
                m.CreateMap<VerifyPageViewModel, ModifyPageViewModel>().ForMember(d => d.Scores, s => s.Ignore());

                m.CreateMap<ScoreModifyViewModel, ScoreModifyViewModel>();
                m.CreateMap<ModifyPageViewModel, ModifyPageViewModel>().ForMember(d => d.Scores, s => s.Ignore());


                m.CreateMap<Class, ClassEntryModel>().IncludeBase<Class, ClassMiniModel>()
                                                     .ForMember(d => d.SubjectCount, s => s.MapFrom(x => x.Subjects.Count()))
                                                     .ForMember(d => d.VerifiedCount, s => s.MapFrom(x => x.Subjects.Count(l => l.VerifiedByID.HasValue)))
                                                     .ForMember(d => d.LastTimeVerified, s => s.MapFrom(x => x.Subjects.Select(l => l.TimeVerified).DefaultIfEmpty(DateTime.MinValue).Max()));


                m.CreateMap<Class, UploadClassScoreViewModel>().ForMember(d => d.Name, s => s.MapFrom(x => x.Arm.Name))
                                                               .ForMember(d => d.SubjectCount, s => s.MapFrom(x => x.Subjects.Count()))
                                                               .ForMember(d => d.VerifiedCount, s => s.MapFrom(x => x.Subjects.Count(l => l.VerifiedByID.HasValue)))
                                                               .ForMember(d => d.LastTimeVerified, s => s.MapFrom(x => x.Subjects.Max(l => l.TimeVerified)));


                m.CreateMap<CommentsViewModel, Student>();
                m.CreateMap<Student, CommentsMiniModel>().ForMember(d => d.ClassName, s => s.MapFrom(x => x.Class.Arm.Name))
                                                         .ForMember(d => d.DisplayName, s => s.MapFrom(x => x.Surname.ToUpper() + " " + x.FirstName));
                m.CreateMap<Student, CommentsViewModel>().IncludeBase<Student, CommentsMiniModel>();

                m.CreateMap<Student, CommentTableModel>().ForMember(d => d.ClassName, s => s.MapFrom(x => x.Class.Arm.Name))
                                                         .ForMember(d => d.DisplayName, s => s.MapFrom(x => x.Surname.ToUpper() + " " + x.FirstName));

                m.CreateMap<Student, CTCommentViewModel>().IncludeBase<Student, CommentTableModel>();
                m.CreateMap<Student, PCommentViewModel>().IncludeBase<Student, CommentTableModel>();

                m.CreateMap<Class, ClassCommentsPageViewModel>().ForMember(d => d.Name, s => s.MapFrom(x => x.Arm.Name))
                                                                .ForMember(d => d.SchoolID, s => s.MapFrom(x => x.Term.SchoolID))
                                                                .ForMember(d => d.TermName, s => s.MapFrom(x => x.Term.Name));



                m.CreateMap<Class, ClassSkillViewModel>().IncludeBase<Class, ClassMiniModel>()
                                                         .ForMember(d => d.SkillGroupName, s => s.MapFrom(x => x.SkillGroup.Name))
                                                         .ForMember(d => d.HasEntries, s => s.MapFrom(x => x.SkillEntries.Any(l => l.SkillScore1.HasValue)));

                m.CreateMap<StudentSkills, SkillEntryViewModel>().ForMember(d => d.DisplayName, s => s.MapFrom(x => x.Student.Surname.ToUpper() + " " + x.Student.FirstName));
                m.CreateMap<SkillEntryViewModel, StudentSkills>();

                m.CreateMap<SkillModifyViewModel, StudentSkills>().ForMember(d => d.SkillScore1, s => s.MapFrom(x => x.SkillScore1M))
                                                                  .ForMember(d => d.SkillScore2, s => s.MapFrom(x => x.SkillScore2M))
                                                                  .ForMember(d => d.SkillScore3, s => s.MapFrom(x => x.SkillScore3M))
                                                                  .ForMember(d => d.SkillScore4, s => s.MapFrom(x => x.SkillScore4M))
                                                                  .ForMember(d => d.SkillScore5, s => s.MapFrom(x => x.SkillScore5M))
                                                                  .ForMember(d => d.SkillScore6, s => s.MapFrom(x => x.SkillScore6M));

                m.CreateMap<SkillModifyViewModel, SkillModifyViewModel>().ForMember(d => d.SkillScore1V, s => s.MapFrom(x => x.SkillScore1M))
                                                                        .ForMember(d => d.SkillScore2V, s => s.MapFrom(x => x.SkillScore2M))
                                                                        .ForMember(d => d.SkillScore3V, s => s.MapFrom(x => x.SkillScore3M))
                                                                        .ForMember(d => d.SkillScore4V, s => s.MapFrom(x => x.SkillScore4M))
                                                                        .ForMember(d => d.SkillScore5V, s => s.MapFrom(x => x.SkillScore5M))
                                                                        .ForMember(d => d.SkillScore6V, s => s.MapFrom(x => x.SkillScore6M))
                                                                        .ForMember(d => d.SkillScore1M, s => s.Ignore())
                                                                        .ForMember(d => d.SkillScore2M, s => s.Ignore())
                                                                        .ForMember(d => d.SkillScore3M, s => s.Ignore())
                                                                        .ForMember(d => d.SkillScore4M, s => s.Ignore())
                                                                        .ForMember(d => d.SkillScore5M, s => s.Ignore())
                                                                        .ForMember(d => d.SkillScore6M, s => s.Ignore());

                m.CreateMap<StudentSkills, SkillVerifyViewModel>().IncludeBase<StudentSkills, SkillEntryViewModel>();
                m.CreateMap<SkillVerifyViewModel, SkillModifyViewModel>();

                m.CreateMap<Class, ClassSkillPageViewModel>().ForMember(d => d.Name, s => s.MapFrom(x => x.Arm.Name))
                                                             .ForMember(d => d.SchoolID, s => s.MapFrom(x => x.Term.SchoolID))
                                                             .ForMember(d => d.TermName, s => s.MapFrom(x => x.Term.Name))
                                                             .ForMember(d => d.Grades, s => s.MapFrom(x => x.SkillGroup.Grades))
                                                             .ForMember(d => d.Skill1, s => s.MapFrom(x => x.SkillGroup.Skill1))
                                                             .ForMember(d => d.Skill2, s => s.MapFrom(x => x.SkillGroup.Skill2))
                                                             .ForMember(d => d.Skill3, s => s.MapFrom(x => x.SkillGroup.Skill3))
                                                             .ForMember(d => d.Skill4, s => s.MapFrom(x => x.SkillGroup.Skill4))
                                                             .ForMember(d => d.Skill5, s => s.MapFrom(x => x.SkillGroup.Skill5))
                                                             .ForMember(d => d.Skill6, s => s.MapFrom(x => x.SkillGroup.Skill6));


                m.CreateMap<Class, ClassSkillMViewModel>().ForMember(d => d.Skill1, s => s.MapFrom(x => x.SkillGroup.Skill1))
                                                        .ForMember(d => d.Skill2, s => s.MapFrom(x => x.SkillGroup.Skill2))
                                                        .ForMember(d => d.Skill3, s => s.MapFrom(x => x.SkillGroup.Skill3))
                                                        .ForMember(d => d.Skill4, s => s.MapFrom(x => x.SkillGroup.Skill4))
                                                        .ForMember(d => d.Skill5, s => s.MapFrom(x => x.SkillGroup.Skill5))
                                                        .ForMember(d => d.Skill6, s => s.MapFrom(x => x.SkillGroup.Skill6));

                m.CreateMap<Class, ClassSkillVPageViewModel>().IncludeBase<Class, ClassSkillPageViewModel>();

                m.CreateMap<ScoreEntry, ScoreSheetEntryViewModel>().ForMember(d => d.StudentName, s => s.MapFrom(x => x.Student.Surname.ToUpper() + " " + x.Student.FirstName));
                m.CreateMap<Subject, SubjectScoreSheetViewModel>().ForMember(d => d.Order, s => s.MapFrom(x => x.Template.Order))
                                                                  .ForMember(d => d.Name, s => s.MapFrom(x => x.Template.Name))
                                                                  .ForMember(d => d.TeacherName, s => s.MapFrom(x => x.Teacher.Name))
                                                                  .ForMember(d => d.Entries, s => s.MapFrom(x => x.Scores));
                m.CreateMap<Class, ClassScoreSheetsPageViewModel>().ForMember(d => d.Name, s => s.MapFrom(x => x.Arm.Name))
                                                                   .ForMember(d => d.SchoolID, s => s.MapFrom(x => x.Term.SchoolID))
                                                                   .ForMember(d => d.TermName, s => s.MapFrom(x => x.Term.Name))
                                                                   .ForMember(d => d.SchoolName, s => s.MapFrom(x => x.Term.School.Name))
                                                                   .ForMember(d => d.StudentCount, s => s.MapFrom(x => x.Students.Count()))
                                                                   .ForMember(d => d.Sheets, s => s.MapFrom(x => x.Subjects));

                m.CreateMap<ScoreEntry, ScoreEntryExcel>().ForMember(d => d.StudentID, s => s.MapFrom(x => x.StudentID))
                                                          .ForMember(d => d.Surname, s => s.MapFrom(x => x.Student.Surname))
                                                          .ForMember(d => d.FirstName, s => s.MapFrom(x => x.Student.FirstName));
                m.CreateMap<Subject, SubjectScoreSheetExcel>().ForMember(d => d.Order, s => s.MapFrom(x => x.Template.Order))
                                                              .ForMember(d => d.Name, s => s.MapFrom(x => x.Template.Name))
                                                              .ForMember(d => d.Entries, s => s.MapFrom(x => x.Scores));
                m.CreateMap<Class, ClassScoreSheetsExcel>().ForMember(d => d.Name, s => s.MapFrom(x => x.Arm.Name))
                                                           .ForMember(d => d.SchoolID, s => s.MapFrom(x => x.Term.SchoolID))
                                                           .ForMember(d => d.SchoolYear, s => s.MapFrom(x => x.Term.SchoolYear))
                                                           .ForMember(d => d.TermNumber, s => s.MapFrom(x => x.Term.TermNumber))
                                                           .ForMember(d => d.Sheets, s => s.MapFrom(x => x.Subjects));



                m.CreateMap<Term, ReportIndexPageViewModel>();

                m.CreateMap<Class, ClassStatsModel>().ForMember(d => d.Name, s => s.MapFrom(x => x.Arm.Name))
                                                     .ForMember(d => d.StudentCount, s => s.MapFrom(x => x.Students.Count()))
                                                     .ForMember(d => d.SubjectCount, s => s.MapFrom(x => (byte)x.Subjects.Count()));

                m.CreateMap<ClassResult, ClassReportModel>().ForMember(d => d.Name, s => s.MapFrom(x => x.Class.Arm.Name))
                                                            .ForMember(d => d.BestStudentName, s => s.MapFrom(x => x.BestStudent.Surname.ToUpper() + " " + x.BestStudent.FirstName))
                                                            .ForMember(d => d.BestSubjectName, s => s.MapFrom(x => x.BestSubject.Template.Name));

                m.CreateMap<Class, AnalyzeClassViewModel>().ForMember(d => d.Name, s => s.MapFrom(x => x.Arm.Name))
                                                           .ForMember(d => d.ClassLevelID, s => s.MapFrom(x => x.Arm.ClassLevelID))
                                                           .ForMember(d => d.ImprovementComments, s => s.MapFrom(x => x.ImprovementCommentGroup.Name))
                                                           .ForMember(d => d.PerformanceComments, s => s.MapFrom(x => x.PerformanceCommentGroup.Name))
                                                           .ForMember(d => d.PromotionComments, s => s.MapFrom(x => x.PromotionCommentGroup.Name));

                m.CreateMap<Class, ClassAnalysisModel>().ForMember(d => d.ClassLevelID, s => s.MapFrom(x => x.Arm.ClassLevelID))
                                                        .ForMember(d => d.SchoolYear, s => s.MapFrom(x => x.Term.SchoolYear))
                                                        .ForMember(d => d.NextResumptionDate, s => s.MapFrom(x => x.Term.NextResumptionDate))
                                                        .ForMember(d => d.LogoGuidString, s => s.MapFrom(x => x.Term.School.GuidString))
                                                        .ForMember(d => d.SchoolID, s => s.MapFrom(x => x.Term.SchoolID))
                                                        .ForMember(d => d.UseImprovementComments, s => s.MapFrom(x => x.ImprovementCommentGroupID.HasValue))
                                                        .ForMember(d => d.UsePerformanceComments, s => s.MapFrom(x => x.PerformanceCommentGroupID.HasValue))
                                                        .ForMember(d => d.UsePromotionComments, s => s.MapFrom(x => x.PromotionCommentGroupID.HasValue))
                                                        .ForMember(d => d.ImprovementComments, s => s.MapFrom(x => x.ImprovementCommentGroup.Comments))
                                                        .ForMember(d => d.PerformanceComments, s => s.MapFrom(x => x.PerformanceCommentGroup.Comments))
                                                        .ForMember(d => d.PromotionComments, s => s.MapFrom(x => x.PromotionCommentGroup.Comments));

                m.CreateMap<Subject, SubjectResultModel>().ForMember(d => d.Order, s => s.MapFrom(x => x.Template.Order));
                m.CreateMap<StudentResult, BroadsheetRowModel>().ForMember(d => d.StudentName, s => s.MapFrom(x => x.Student.Surname.ToUpper() + " " + x.Student.FirstName));

                m.CreateMap<ClassResult, ClassResultMiniModel>().ForMember(d => d.Name, s => s.MapFrom(x => x.Class.Arm.Name))
                                                               .ForMember(d => d.TermName, s => s.MapFrom(x => x.Class.Term.Name))
                                                               .ForMember(d => d.SchoolID, s => s.MapFrom(x => x.Class.Term.SchoolID))
                                                               .ForMember(d => d.SchoolName, s => s.MapFrom(x => x.Class.Term.School.Name))
                                                               .ForMember(d => d.BestSubject, s => s.MapFrom(x => x.BestSubject.Template.Name))
                                                               .ForMember(d => d.BestStudent, s => s.MapFrom(x => x.BestStudent.Surname.ToUpper() + " " + x.BestStudent.FirstName));

                m.CreateMap<ClassResult, BroadsheetViewModel>().IncludeBase<ClassResult, ClassResultMiniModel>()
                                                               .ForMember(d => d.RedLine, s => s.MapFrom(x => x.Class.RedLine))
                                                               .ForMember(d => d.Scores, s => s.MapFrom(x => x.Class.Subjects.SelectMany(l => l.Scores)))
                                                               .ForMember(d => d.Subjects, s => s.MapFrom(x => x.Class.Subjects));

                m.CreateMap<ClassResult, GradeBroadsheetViewModel>().IncludeBase<ClassResult, BroadsheetViewModel>()
                                                                    .ForMember(d => d.ShowSummaryGrade, s => s.MapFrom(x => x.Class.ShowSummaryGrade))
                                                                    .ForMember(d => d.Scores, s => s.MapFrom(x => x.Class.Subjects.SelectMany(l => l.Scores)));

                m.CreateMap<ClassResult, FullBroadsheetViewModel>().IncludeBase<ClassResult, GradeBroadsheetViewModel>()
                                                                   .ForMember(d => d.Scores, s => s.MapFrom(x => x.Class.Subjects.SelectMany(l => l.Scores)));


                m.CreateMap<ScoreEntry, ScoreSummaryGradeModel>().ForMember(d => d.SummaryGradeID, s => s.MapFrom(x => x.Grade.SummaryGradeID));

                m.CreateMap<ScoreEntry, ScoreEntryGradeModel>().ForMember(d => d.GradeName, s => s.MapFrom(x => x.Grade.Name))
                                                               .ForMember(d => d.SummaryGradeID, s => s.MapFrom(x => x.Grade.SummaryGradeID));
                m.CreateMap<ScoreEntry, ScoreEntryFullModel>().IncludeBase<ScoreEntry, ScoreEntryGradeModel>();
                m.CreateMap<ScoreEntry, ScoreEntryRepModel>().ForMember(d => d.SummaryGradeID, s => s.MapFrom(x => x.Grade.SummaryGradeID));


                m.CreateMap<Subject, SubjectPerformanceModel>().ForMember(d => d.Name, s => s.MapFrom(x => x.Template.Name))
                                                               .ForMember(d => d.Order, s => s.MapFrom(x => x.Template.Order))
                                                               .ForMember(d => d.CategoryID, s => s.MapFrom(x => x.Template.CategoryID));

                m.CreateMap<ClassResult, ClassPerformanceModel>().IncludeBase<ClassResult, ClassResultMiniModel>()
                                                                 .ForMember(d => d.Scores, s => s.MapFrom(x => x.Class.Subjects.SelectMany(l => l.Scores)))
                                                                 .ForMember(d => d.Students, s => s.MapFrom(x => x.Class.Results))
                                                                 .ForMember(d => d.Subjects, s => s.MapFrom(x => x.Class.Subjects))
                                                                 .ForMember(d => d.IsPromotionalClass, s => s.MapFrom(x => x.Class.IsPromotionalClass))
                                                                 .ForMember(d => d.SchoolYear, s => s.MapFrom(x => x.Class.Term.SchoolYear))
                                                                 .ForMember(d => d.ShowSummaryGrade, s => s.MapFrom(x => x.Class.ShowSummaryGrade))
                                                                 .ForMember(d => d.Grades, s => s.MapFrom(x => x.Class.GradeGroup.Grades));

                m.CreateMap<StudentResult, StudentMiniModel>().ForMember(d => d.FirstName, s => s.MapFrom(x => x.Student.FirstName))
                                                              .ForMember(d => d.Surname, s => s.MapFrom(x => x.Student.Surname));

                m.CreateMap<StudentResult, StudentTermResultModel>().ForMember(d => d.TermNumber, s => s.MapFrom(x => x.Class.Term.TermNumber));

                m.CreateMap<StudentResult, StudentReportModel>().ForMember(d => d.FirstName, s => s.MapFrom(x => x.Student.FirstName))
                                                               .ForMember(d => d.Surname, s => s.MapFrom(x => x.Student.Surname))
                                                               .ForMember(d => d.StudentCode, s => s.MapFrom(x => x.Student.StudentCode))
                                                               .ForMember(d => d.ClassName, s => s.MapFrom(x => x.Class.Arm.Name))
                                                               .ForMember(d => d.CAWeight, s => s.MapFrom(x => x.Class.CAWeight))
                                                               .ForMember(d => d.ExamWeight, s => s.MapFrom(x => x.Class.ExamWeight))
                                                               .ForMember(d => d.ShowPosition, s => s.MapFrom(x => x.Class.ShowPosition))
                                                               .ForMember(d => d.HideClassAverage, s => s.MapFrom(x => x.Class.HideClassAverage))
                                                               .ForMember(d => d.ShowSummaryGrade, s => s.MapFrom(x => x.Class.ShowSummaryGrade))
                                                               .ForMember(d => d.ShowYearResult, s => s.MapFrom(x => x.Class.ShowYearResult))
                                                               .ForMember(d => d.ShowCategoryAnalysis, s => s.MapFrom(x => x.Class.ShowCategoryAnalysis))
                                                               .ForMember(d => d.RedLine, s => s.MapFrom(x => x.Class.RedLine))
                                                               .ForMember(d => d.Grades, s => s.MapFrom(x => x.Class.GradeGroup.Grades))
                                                               .ForMember(d => d.TermName, s => s.MapFrom(x => x.Class.Term.Name))
                                                               .ForMember(d => d.SchoolYear, s => s.MapFrom(x => x.Class.Term.SchoolYear))
                                                               .ForMember(d => d.TermNumber, s => s.MapFrom(x => x.Class.Term.TermNumber))
                                                               .ForMember(d => d.NextResumptionDate, s => s.MapFrom(x => x.Class.Term.NextResumptionDate))
                                                               .ForMember(d => d.Skill1, s => s.MapFrom(x => x.Class.SkillGroup.Skill1))
                                                               .ForMember(d => d.Skill2, s => s.MapFrom(x => x.Class.SkillGroup.Skill2))
                                                               .ForMember(d => d.Skill3, s => s.MapFrom(x => x.Class.SkillGroup.Skill3))
                                                               .ForMember(d => d.Skill4, s => s.MapFrom(x => x.Class.SkillGroup.Skill4))
                                                               .ForMember(d => d.Skill5, s => s.MapFrom(x => x.Class.SkillGroup.Skill5))
                                                               .ForMember(d => d.Skill6, s => s.MapFrom(x => x.Class.SkillGroup.Skill6))
                                                               .ForMember(d => d.HasSkills, s => s.MapFrom(x => x.Class.SkillGroupID.HasValue))
                                                               .ForMember(d => d.SkillGrades, s => s.MapFrom(x => x.Class.SkillGroup.Grades))
                                                               .ForMember(d => d.SchoolID, s => s.MapFrom(x => x.Student.SchoolID))
                                                               .ForMember(d => d.SchoolName, s => s.MapFrom(x => x.Student.School.Name))
                                                               .ForMember(d => d.SchoolTypeID, s => s.MapFrom(x => x.Student.School.TypeID))
                                                               .ForMember(d => d.LogoGuidString, s => s.MapFrom(x => x.Class.Term.GuidString))
                                                               .ForMember(d => d.BestSubjectName, s => s.MapFrom(x => x.BestSubject.ResultName));
                m.CreateMap<Subject, StudentSubjectModel>().IncludeBase<Subject, SubjectResultModel>()
                                                           .ForMember(d => d.CategoryID, s => s.MapFrom(x => x.Template.CategoryID))
                                                           .ForMember(d => d.NoCA, s => s.MapFrom(x => x.NoCA));
                m.CreateMap<ScoreEntry, ScoreReportModel>().IncludeBase<ScoreEntry, ScoreEntryFullModel>()
                                                           .ForMember(d => d.TemplateID, s => s.MapFrom(x => x.Subject.TemplateID))
                                                           .ForMember(d => d.TermNumber, s => s.MapFrom(x => x.Subject.TermNumber));
                m.CreateMap<SkillGrade, SkillGradeResultModel>();
                m.CreateMap<StudentSkills, StudentSkillsReportModel>();


                m.CreateMap<StudentResult, StudentResultViewModel>().ForMember(d => d.DisplayName, s => s.MapFrom(x => x.Student.Surname.ToUpper() + " " + x.Student.FirstName))
                                                                   .ForMember(d => d.ClassName, s => s.MapFrom(x => x.Class.Arm.Name))
                                                                   .ForMember(d => d.TermName, s => s.MapFrom(x => x.Class.Term.Name))
                                                                   .ForMember(d => d.BestSubjectName, s => s.MapFrom(x => x.BestSubject.ResultName));


                m.CreateMap<School, OtherExamTypePageViewModel>().ForMember(d => d.SchoolName, s => s.MapFrom(x => x.Name))
                                                                 .ForMember(d => d.LocationName, s => s.MapFrom(x => x.Location.Name))
                                                                 .ForMember(d => d.SchoolTypeID, s => s.MapFrom(x => x.TypeID));

                m.CreateMap<OtherExamTypeViewModel, OtherExamType>();
                m.CreateMap<OtherExamType, OtherExamTypeViewModel>().ForMember(d => d.ExamCount, s => s.MapFrom(x => x.Exams.Count));

                m.CreateMap<OtherExam, OtherExamViewModel>().ForMember(d => d.Name, s => s.MapFrom(x => x.Type.Name))
                                                            .ForMember(d => d.ClassName, s => s.MapFrom(x => x.Class.Arm.Name))
                                                            .ForMember(d => d.BestStudentName, s => s.MapFrom(x => x.BestStudent.Surname.ToUpper() + " " + x.BestStudent.FirstName))
                                                            .ForMember(d => d.BestSubjectName, s => s.MapFrom(x => x.BestSubject.Template.Name));

                m.CreateMap<Class, OtherExamCreateViewModel>().ForMember(d => d.ClassName, s => s.MapFrom(x => x.Arm.Name));

                m.CreateMap<OtherExam, OtherExamAnalysisModel>().ForMember(d => d.ExamName, s => s.MapFrom(x => x.Type.Name))
                                                                .ForMember(d => d.ClassName, s => s.MapFrom(x => x.Class.Arm.Name))
                                                                .ForMember(d => d.Subjects, s => s.MapFrom(x => x.Class.Subjects));

                m.CreateMap<ScoreEntryDataModel, OtherExamScore>().ForMember(d => d.Score, s => s.MapFrom(x => x.Total));

                m.CreateMap<OtherExam, ClassResultMiniModel>().ForMember(d => d.Name, s => s.MapFrom(x => x.Class.Arm.Name + " - " + x.Type.Name))
                                                               .ForMember(d => d.TermName, s => s.MapFrom(x => x.Class.Term.Name))
                                                               .ForMember(d => d.SchoolID, s => s.MapFrom(x => x.Class.Term.SchoolID))
                                                               .ForMember(d => d.SchoolName, s => s.MapFrom(x => x.Class.Term.School.Name))
                                                               .ForMember(d => d.BestSubject, s => s.MapFrom(x => x.BestSubject.Template.Name))
                                                               .ForMember(d => d.BestStudent, s => s.MapFrom(x => x.BestStudent.Surname.ToUpper() + " " + x.BestStudent.FirstName));

                m.CreateMap<OtherExam, BroadsheetViewModel>().IncludeBase<OtherExam, ClassResultMiniModel>()
                                                             .ForMember(d => d.RedLine, s => s.MapFrom(x => x.Class.RedLine))
                                                             .ForMember(d => d.Scores, s => s.MapFrom(x => x.Class.Subjects.SelectMany(l => l.OtherExamScores)))
                                                             .ForMember(d => d.Subjects, s => s.MapFrom(x => x.Class.Subjects));

                m.CreateMap<OtherExam, GradeBroadsheetViewModel>().IncludeBase<OtherExam, BroadsheetViewModel>()
                                                                  .ForMember(d => d.ShowSummaryGrade, s => s.MapFrom(x => x.Class.ShowSummaryGrade))
                                                                  .ForMember(d => d.Scores, s => s.MapFrom(x => x.Class.Subjects.SelectMany(l => l.OtherExamScores)));

                m.CreateMap<OtherExam, FullBroadsheetViewModel>().IncludeBase<OtherExam, GradeBroadsheetViewModel>()
                                                                 .ForMember(d => d.Scores, s => s.MapFrom(x => x.Class.Subjects.SelectMany(l => l.OtherExamScores)));

                m.CreateMap<OtherExamResult, BroadsheetRowModel>().ForMember(d => d.StudentName, s => s.MapFrom(x => x.Student.Surname.ToUpper() + " " + x.Student.FirstName));
                m.CreateMap<OtherExamScore, ExamScoreReportModel>().ForMember(d => d.Total, s => s.MapFrom(x => x.Score))
                                                                   .ForMember(d => d.GradeName, s => s.MapFrom(x => x.Grade.Name))
                                                                   .ForMember(d => d.SummaryGradeID, s => s.MapFrom(x => x.Grade.SummaryGradeID));
                m.CreateMap<OtherExamScore, ScoreEntryModel>().ForMember(d => d.ExamScore, s => s.MapFrom(x => x.Score));
                m.CreateMap<OtherExamScore, ScoreEntryGradeModel>().ForMember(d => d.GradeName, s => s.MapFrom(x => x.Grade.Name))
                                                                   .ForMember(d => d.SummaryGradeID, s => s.MapFrom(x => x.Grade.SummaryGradeID));
                m.CreateMap<OtherExamScore, ScoreEntryFullModel>().IncludeBase<OtherExamScore, ScoreEntryGradeModel>()
                                                                  .ForMember(d => d.ExamScore, s => s.MapFrom(x => x.Score));

                m.CreateMap<OtherExamResult, OtherExamReportModel>().ForMember(d => d.FirstName, s => s.MapFrom(x => x.Student.FirstName))
                                                                   .ForMember(d => d.Surname, s => s.MapFrom(x => x.Student.Surname))
                                                                   .ForMember(d => d.StudentCode, s => s.MapFrom(x => x.Student.StudentCode))
                                                                   .ForMember(d => d.GuidString, s => s.MapFrom(x => x.Student.GuidString))
                                                                   .ForMember(d => d.StudentClassID, s => s.MapFrom(x => x.Student.ClassID))
                                                                   .ForMember(d => d.ClassID, s => s.MapFrom(x => x.Exam.ClassID))
                                                                   .ForMember(d => d.ClassName, s => s.MapFrom(x => x.Exam.Class.Arm.Name))
                                                                   .ForMember(d => d.ExamName, s => s.MapFrom(x => x.Exam.Type.Name))
                                                                   .ForMember(d => d.ShowPosition, s => s.MapFrom(x => x.Exam.Class.ShowPosition))
                                                                   .ForMember(d => d.ShowSummaryGrade, s => s.MapFrom(x => x.Exam.Class.ShowSummaryGrade))
                                                                   .ForMember(d => d.RedLine, s => s.MapFrom(x => x.Exam.Class.RedLine))
                                                                   .ForMember(d => d.Grades, s => s.MapFrom(x => x.Exam.Class.GradeGroup.Grades))
                                                                   .ForMember(d => d.TermName, s => s.MapFrom(x => x.Exam.Class.Term.Name))
                                                                   .ForMember(d => d.SchoolID, s => s.MapFrom(x => x.Student.SchoolID))
                                                                   .ForMember(d => d.SchoolName, s => s.MapFrom(x => x.Student.School.Name))
                                                                   .ForMember(d => d.SchoolTypeID, s => s.MapFrom(x => x.Student.School.TypeID))
                                                                   .ForMember(d => d.LogoGuidString, s => s.MapFrom(x => x.Exam.Class.Term.GuidString))
                                                                   .ForMember(d => d.BestSubjectName, s => s.MapFrom(x => x.BestSubject.ResultName));

                m.CreateMap<OtherExamResult, OtherExamResultViewModel>().ForMember(d => d.DisplayName, s => s.MapFrom(x => x.Student.Surname.ToUpper() + " " + x.Student.FirstName))
                                                                        .ForMember(d => d.ClassName, s => s.MapFrom(x => x.Exam.Class.Arm.Name))
                                                                        .ForMember(d => d.ExamName, s => s.MapFrom(x => x.Exam.Type.Name))
                                                                        .ForMember(d => d.BestSubjectName, s => s.MapFrom(x => x.BestSubject.ResultName));



                m.CreateMap<Subject, SubjectReportMiniModel>().ForMember(d => d.Name, s => s.MapFrom(x => x.Template.Name))
                                                              .ForMember(d => d.ClassLevelID, s => s.MapFrom(x => x.Template.ClassLevelID))
                                                              .ForMember(d => d.CategoryID, s => s.MapFrom(x => x.Template.CategoryID));

                m.CreateMap<Subject, SubjectReportViewModel>().ForMember(d => d.Name, s => s.MapFrom(x => x.Template.Name))
                                                              .ForMember(d => d.CategoryID, s => s.MapFrom(x => x.Template.CategoryID))
                                                              .ForMember(d => d.ClassName, s => s.MapFrom(x => x.Class.Arm.Name));

                m.CreateMap<Subject, SubjectReportModel>().ForMember(d => d.Name, s => s.MapFrom(x => x.Template.Name))
                                                          .ForMember(d => d.CategoryID, s => s.MapFrom(x => x.Template.CategoryID))
                                                          .ForMember(d => d.TeacherName, s => s.MapFrom(x => x.Teacher.Name))
                                                          .ForMember(d => d.ClassName, s => s.MapFrom(x => x.Class.Arm.Name))
                                                          .ForMember(d => d.TermName, s => s.MapFrom(x => x.Class.Term.Name))
                                                          .ForMember(d => d.ShowSummaryGrade, s => s.MapFrom(x => x.Class.ShowSummaryGrade))
                                                          .ForMember(d => d.CAWeight, s => s.MapFrom(x => x.Class.CAWeight))
                                                          .ForMember(d => d.ExamWeight, s => s.MapFrom(x => x.Class.ExamWeight))
                                                          .ForMember(d => d.RedLine, s => s.MapFrom(x => x.Class.RedLine))
                                                          .ForMember(d => d.Grades, s => s.MapFrom(x => x.Class.GradeGroup.Grades))
                                                          .ForMember(d => d.SchoolID, s => s.MapFrom(x => x.Template.SchoolID))
                                                          .ForMember(d => d.SchoolName, s => s.MapFrom(x => x.Class.Term.School.Name));

                m.CreateMap<ScoreEntry, SubjectScoreFullModel>().IncludeBase<ScoreEntry, ScoreEntryFullModel>()
                                                                .ForMember(d => d.FirstName, s => s.MapFrom(x => x.Student.FirstName))
                                                                .ForMember(d => d.Surname, s => s.MapFrom(x => x.Student.Surname))
                                                                .ForMember(d => d.StudentCode, s => s.MapFrom(x => x.Student.StudentCode))
                                                                .ForMember(d => d.IsMale, s => s.MapFrom(x => x.Student.IsMale));


                m.CreateMap<Term, TermReportPageViewModel>().ForMember(d => d.TermName, s => s.MapFrom(x => x.Name))
                                                            .ForMember(d => d.ClassCount, s => s.MapFrom(x => x.Classes.Count()))
                                                            .ForMember(d => d.DefSubjectCount, s => s.MapFrom(x => x.Classes.Sum(c => c.Subjects.Count())))
                                                            .ForMember(d => d.StudentCount, s => s.MapFrom(x => x.Classes.Sum(c => c.Students.Count())));

                m.CreateMap<TermResult, TermReportPageViewModel>().ForMember(d => d.TermName, s => s.MapFrom(x => x.Term.Name))
                                                                  .ForMember(d => d.NextResumptionDate, s => s.MapFrom(x => x.Term.NextResumptionDate))
                                                                  .ForMember(d => d.BestStudentName, s => s.MapFrom(x => x.BestStudent.Surname.ToUpper() + " " + x.BestStudent.FirstName))
                                                                  .ForMember(d => d.BestSubjectName, s => s.MapFrom(x => x.BestSubject.Class.Arm.Name + " " + x.BestSubject.ResultName))
                                                                  .ForMember(d => d.BestClassName, s => s.MapFrom(x => x.BestClass.Class.Arm.Name))
                                                                  .ForMember(d => d.BestClassAverage, s => s.MapFrom(x => x.BestClass.MeanAverage))
                                                                  .ForMember(d => d.BestClassSize, s => s.MapFrom(x => x.BestClass.Class.Results.Count()))
                                                                  .ForMember(d => d.ClassCount, s => s.MapFrom(x => x.Term.Classes.Count()))
                                                                  .ForMember(d => d.DefSubjectCount, s => s.MapFrom(x => x.Term.Classes.Sum(c => c.Subjects.Count())))
                                                                  .ForMember(d => d.StudentCount, s => s.MapFrom(x => x.Term.Classes.Sum(c => c.Students.Count())));

                m.CreateMap<TermSubjectCategoryStats, SubjectCategoryStatsViewModel>().ForMember(d => d.BestSubjectName, s => s.MapFrom(x => x.BestSubject.Class.Arm.Name + " " + x.BestSubject.ResultName))
                                                                                      .ForMember(d => d.BestStudentName, s => s.MapFrom(x => x.BestStudent.Surname.ToUpper() + " " + 
                                                                                                                                             x.BestStudent.FirstName));
                m.CreateMap<TermSubjectCategoryStats, TermCategoryStatsModel>().IncludeBase<TermSubjectCategoryStats, SubjectCategoryStatsViewModel>();
                m.CreateMap<TermSubjectCategoryLevelStats, LevelCategoryStatsModel>().ForMember(d => d.BestStudentAverage, s => s.MapFrom(x => x.BestAverage))
                                                                                     .ForMember(d => d.BestStudentName, s => s.MapFrom(x => x.BestStudent.Surname.ToUpper() + " " +
                                                                                                                                             x.BestStudent.FirstName));

                m.CreateMap<TermResult, TermPerformanceModel>().ForMember(d => d.SchoolID, s => s.MapFrom(x => x.Term.SchoolID))
                                                                .ForMember(d => d.TermName, s => s.MapFrom(x => x.Term.Name))
                                                                .ForMember(d => d.BestStudentName, s => s.MapFrom(x => x.BestStudent.Surname.ToUpper() + " " + x.BestStudent.FirstName))
                                                                .ForMember(d => d.BestSubjectName, s => s.MapFrom(x => x.BestSubject.Class.Arm.Name + " " + x.BestSubject.Template.Name))
                                                                .ForMember(d => d.BestClassName, s => s.MapFrom(x => x.BestClass.Class.Arm.Name))
                                                                .ForMember(d => d.BestClassAverage, s => s.MapFrom(x => x.BestClass.MeanAverage))
                                                                .ForMember(d => d.BestClassSize, s => s.MapFrom(x => x.BestClass.Class.Results.Count()));


                m.CreateMap<TermSubjectCategoryStats, TermCategoryPerformanceModel>().ForMember(d => d.BestSubjectName, s => s.MapFrom(x => x.BestSubject.Class.Arm.Name + " " + x.BestSubject.ResultName))
                                                                                     .ForMember(d => d.BestStudentName, s => s.MapFrom(x => x.BestStudent.Surname.ToUpper() + " " +
                                                                                                                                            x.BestStudent.FirstName));

                m.CreateMap<TermSubjectStats, SubjectStatsModel>().ForMember(d => d.Name, s => s.MapFrom(x => x.Template.Name))
                                                                  .ForMember(d => d.ResultName, s => s.MapFrom(x => x.Template.ResultName))
                                                                  .ForMember(d => d.LevelID, s => s.MapFrom(x => x.Template.ClassLevelID))
                                                                  .ForMember(d => d.BestStudentName, s => s.MapFrom(x => x.BestStudent.Surname.ToUpper() + " " + x.BestStudent.FirstName));

                m.CreateMap<TermResult, TermResultModel>().ForMember(d => d.TermName, s => s.MapFrom(x => x.Term.Name))
                                                          .ForMember(d => d.SchoolYear, s => s.MapFrom(x => x.Term.SchoolYear))
                                                          .ForMember(d => d.TermNumber, s => s.MapFrom(x => x.Term.TermNumber))
                                                          .ForMember(d => d.BestStudentName, s => s.MapFrom(x => x.BestStudent.Surname.ToUpper() + " " + x.BestStudent.FirstName))
                                                          .ForMember(d => d.BestSubjectName, s => s.MapFrom(x => x.BestSubject.Class.Arm.Name + " " + x.BestSubject.Template.Name))
                                                          .ForMember(d => d.BestClassName, s => s.MapFrom(x => x.BestClass.Class.Arm.Name));

                m.CreateMap<SubjectTemplate, TemplateReportModel>();

                m.CreateMap<TermSubjectStats, TermSubjectStatsModel>().ForMember(d => d.TermName, s => s.MapFrom(x => x.Term.Name))
                                                                      .ForMember(d => d.SchoolYear, s => s.MapFrom(x => x.Term.SchoolYear))
                                                                      .ForMember(d => d.TermNumber, s => s.MapFrom(x => x.Term.TermNumber))
                                                                      .ForMember(d => d.BestStudentName, s => s.MapFrom(x => x.BestStudent.Surname.ToUpper() + " " + x.BestStudent.FirstName));

                m.CreateMap<TermSubjectStats, SubjectStatsRowModel>().IncludeBase<TermSubjectStats, SubjectStatsModel>()
                                                                     .ForMember(d => d.Order, s => s.MapFrom(x => x.Template.Order));

                m.CreateMap<ClassResult, ClassReportMaxModel>().IncludeBase<ClassResult, ClassReportModel>()
                                                               .ForMember(d => d.TermName, s => s.MapFrom(x => x.Class.Term.Name))
                                                               .ForMember(d => d.TermNumber, s => s.MapFrom(x => x.Class.Term.TermNumber))
                                                               .ForMember(d => d.SchoolYear, s => s.MapFrom(x => x.Class.Term.SchoolYear));

                m.CreateMap<ClassResult, ClassReportMaxiModel>().IncludeBase<ClassResult, ClassReportMaxModel>()
                                                               .ForMember(d => d.ClassLevelID, s => s.MapFrom(x => x.Class.Arm.ClassLevelID))
                                                               .ForMember(d => d.ClassTypeID, s => s.MapFrom(x => x.Class.Arm.ClassTypeID));

                m.CreateMap<ClassReportMaxiModel, ClassReportMaxModel>();
                m.CreateMap<ClassTypeStats, ClassTypeStatsModel>();
                m.CreateMap<ClassResult, ClassTypeAnalysisModel>().ForMember(d => d.ClassLevelID, s => s.MapFrom(x => x.Class.Arm.ClassLevelID))
                                                                     .ForMember(d => d.ClassTypeID, s => s.MapFrom(x => x.Class.Arm.ClassTypeID));

                m.CreateMap<Term, SchoolTermMiniModel>().ForMember(d => d.SchoolName, s => s.MapFrom(x => x.School.Name));
                m.CreateMap<ClassSkillSheetsViewModel, ClassSkillSheetsViewModel>().ForMember(d => d.Page, s => s.Ignore())
                                                                                   .ForMember(d => d.Entries, s => s.Ignore());
                m.CreateMap<Class, ClassSkillSheetsViewModel>().ForMember(d => d.Name, s => s.MapFrom(x => x.Arm.Name))
                                                               .ForMember(d => d.Skill1, s => s.MapFrom(x => x.SkillGroup.Skill1))
                                                               .ForMember(d => d.Skill2, s => s.MapFrom(x => x.SkillGroup.Skill2))
                                                               .ForMember(d => d.Skill3, s => s.MapFrom(x => x.SkillGroup.Skill3))
                                                               .ForMember(d => d.Skill4, s => s.MapFrom(x => x.SkillGroup.Skill4))
                                                               .ForMember(d => d.Skill5, s => s.MapFrom(x => x.SkillGroup.Skill5))
                                                               .ForMember(d => d.Skill6, s => s.MapFrom(x => x.SkillGroup.Skill6))
                                                               .ForMember(d => d.HasSkills, s => s.MapFrom(x => x.SkillGroupID.HasValue))
                                                               .ForMember(d => d.Grades, s => s.MapFrom(x => x.SkillGroup.Grades))
                                                               .ForMember(d => d.Entries, s => s.MapFrom(x => x.SkillEntries));
                m.CreateMap<StudentSkills, ScoreSheetEntryViewModel>().ForMember(d => d.StudentName, s => s.MapFrom(x => x.Student.Surname.ToUpper() + " " + x.Student.FirstName));
                m.CreateMap<Student, ScoreSheetEntryViewModel>().ForMember(d => d.StudentName, s => s.MapFrom(x => x.Surname.ToUpper() + " " + x.FirstName));

                m.CreateMap<Class, ClassCommentSheetsViewModel>().ForMember(d => d.Name, s => s.MapFrom(x => x.Arm.Name))
                                                                 .ForMember(d => d.Entries, s => s.MapFrom(x => x.Students));
                m.CreateMap<ClassCommentSheetsViewModel, ClassCommentSheetsViewModel>().ForMember(d => d.Page, s => s.Ignore())
                                                                                       .ForMember(d => d.Entries, s => s.Ignore());

                m.CreateMap<Term, CommentSheetsExcel>().ForMember(d => d.SchoolID, s => s.MapFrom(x => x.SchoolID))
                                                       .ForMember(d => d.SchoolYear, s => s.MapFrom(x => x.SchoolYear))
                                                       .ForMember(d => d.TermNumber, s => s.MapFrom(x => x.TermNumber))
                                                       .ForMember(d => d.Sheets, s => s.MapFrom(x => x.Classes));

                m.CreateMap<Class, ClassCommentSheetExcel>().ForMember(d => d.Name, s => s.MapFrom(x => x.Arm.Name))
                                                            .ForMember(d => d.ClassLevelID, s => s.MapFrom(x => x.Arm.ClassLevelID))
                                                            .ForMember(d => d.Entries, s => s.MapFrom(x => x.Students));

                m.CreateMap<Student, ScoreEntryExcel>();


                m.CreateMap<Term, UploadCommentsViewModel>().ForMember(d => d.ClassCount, s => s.MapFrom(x => x.Classes.Count()))
                                                            .ForMember(d => d.StudentCount, s => s.MapFrom(x => x.Classes.Sum(l => l.Students.Count)));


                m.CreateMap<Term, SchoolTermViewModel>().ForMember(d => d.SchoolName, s => s.MapFrom(x => x.School.Name))
                                                        .ForMember(d => d.ClassCount, s => s.MapFrom(x => x.Classes.Count()))
                                                        .ForMember(d => d.StudentCount, s => s.MapFrom(x => x.Classes.Sum(c => c.Students.Count())))
                                                        .ForMember(d => d.SubjectCount, s => s.MapFrom(x => x.Classes.Sum(c => c.Subjects.Count())))
                                                        .ForMember(d => d.ResultCount, s => s.MapFrom(x => x.Classes.Sum(c => c.Results.Count())));

                m.CreateMap<Student, ResultLabelModel>().ForMember(d => d.ClassName, s => s.MapFrom(x => x.Class.Arm.Name));




                m.CreateMap<StudentResult, StudentReportMiniModel>().ForMember(d => d.TermNumber, s => s.MapFrom(x => x.BestSubject.TermNumber))
                                                                    .ForMember(d => d.Year, s => s.MapFrom(x => x.BestSubject.SchoolYear));


                m.CreateMap<Student, TranscriptViewModel>().ForMember(d => d.CYear, s => s.MapFrom(x => x.Class.Term.SchoolYear))
                                                           .ForMember(d => d.CLevelID, s => s.MapFrom(x => x.Class.Arm.ClassLevelID));

                m.CreateMap<Student, TranscriptPrintModel>().ForMember(d => d.SchoolName, s => s.MapFrom(x => x.School.Name))
                                                            .ForMember(d => d.SchoolTypeID, s => s.MapFrom(x => x.School.TypeID))
                                                            .ForMember(d => d.LogoGuidString, s => s.MapFrom(x => x.School.GuidString));

                m.CreateMap<ScoreEntry, TranscriptScoreModel>().ForMember(d => d.TemplateID, s => s.MapFrom(x => x.Subject.TemplateID))
                                                               .ForMember(d => d.TermNumber, s => s.MapFrom(x => x.Subject.TermNumber));


            });
        }

    }
}