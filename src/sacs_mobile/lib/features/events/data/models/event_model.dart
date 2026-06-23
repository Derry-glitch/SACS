class EventModel {
  final int id;
  final String title;
  final String? description;
  final int courseId;
  final String courseCode;
  final String eventType; // Assignment, Quiz, Exam, Project, StudySession
  final DateTime dueDateTime;
  final String priorityLevel; // Low, Medium, High, Critical
  final String? notes;
  final String? attachmentUrl;
  final int? durationMinutes;
  final String? venue;
  final String? seatNumber;
  final String? supervisorName;
  final int? progressPercentage;
  final String? studyTopic;
  final int? studyDuration;

  EventModel({
    required this.id,
    required this.title,
    this.description,
    required this.courseId,
    required this.courseCode,
    required this.eventType,
    required this.dueDateTime,
    required this.priorityLevel,
    this.notes,
    this.attachmentUrl,
    this.durationMinutes,
    this.venue,
    this.seatNumber,
    this.supervisorName,
    this.progressPercentage,
    this.studyTopic,
    this.studyDuration,
  });

  factory EventModel.fromJson(Map<String, dynamic> json) {
    return EventModel(
      id: json['id'] as int,
      title: json['title'] as String,
      description: json['description'] as String?,
      courseId: json['courseId'] as int,
      courseCode: json['courseCode'] as String,
      eventType: json['eventType'].toString(),
      dueDateTime: DateTime.parse(json['dueDateTime'] as String),
      priorityLevel: json['priorityLevel'] as String,
      notes: json['notes'] as String?,
      attachmentUrl: json['attachmentUrl'] as String?,
      durationMinutes: json['durationMinutes'] as int?,
      venue: json['venue'] as String?,
      seatNumber: json['seatNumber'] as String?,
      supervisorName: json['supervisorName'] as String?,
      progressPercentage: json['progressPercentage'] as int?,
      studyTopic: json['studyTopic'] as String?,
      studyDuration: json['studyDuration'] as int?,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'title': title,
      'description': description,
      'courseId': courseId,
      'courseCode': courseCode,
      'eventType': eventType,
      'dueDateTime': dueDateTime.toIso8601String(),
      'priorityLevel': priorityLevel,
      'notes': notes,
      'attachmentUrl': attachmentUrl,
      'durationMinutes': durationMinutes,
      'venue': venue,
      'seatNumber': seatNumber,
      'supervisorName': supervisorName,
      'progressPercentage': progressPercentage,
      'studyTopic': studyTopic,
      'studyDuration': studyDuration,
    };
  }
}
