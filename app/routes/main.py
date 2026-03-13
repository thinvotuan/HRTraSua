from flask import Blueprint, render_template
from app.models import Employee, Department, AttendanceRecord, LeaveRequest, PayrollRecord
from datetime import date, timedelta
from sqlalchemy import func

main_bp = Blueprint('main', __name__)


@main_bp.route('/')
def index():
    today = date.today()
    total_employees = Employee.query.filter_by(status='active').count()
    total_departments = Department.query.count()

    # Attendance today
    present_today = AttendanceRecord.query.filter_by(
        date=today, status='present'
    ).count()
    late_today = AttendanceRecord.query.filter_by(
        date=today, status='late'
    ).count()
    absent_today = total_employees - AttendanceRecord.query.filter(
        AttendanceRecord.date == today
    ).count()

    # Pending leaves
    pending_leaves = LeaveRequest.query.filter_by(status='pending').count()

    # Recent employees
    recent_employees = Employee.query.order_by(Employee.created_at.desc()).limit(5).all()

    # Attendance this month
    first_day = today.replace(day=1)
    monthly_attendance = AttendanceRecord.query.filter(
        AttendanceRecord.date >= first_day,
        AttendanceRecord.date <= today
    ).count()

    return render_template(
        'index.html',
        total_employees=total_employees,
        total_departments=total_departments,
        present_today=present_today,
        late_today=late_today,
        absent_today=absent_today,
        pending_leaves=pending_leaves,
        recent_employees=recent_employees,
        monthly_attendance=monthly_attendance,
        today=today,
    )
