from flask import Blueprint, render_template, request, redirect, url_for, flash
from app import db
from app.models import LeaveRequest, Employee
from datetime import date

leaves_bp = Blueprint('leaves', __name__)


@leaves_bp.route('/')
def index():
    status = request.args.get('status', '')
    emp_id = request.args.get('emp_id', '')

    query = LeaveRequest.query
    if status:
        query = query.filter_by(status=status)
    if emp_id:
        query = query.filter_by(employee_id=int(emp_id))

    leaves = query.order_by(LeaveRequest.created_at.desc()).all()
    employees = Employee.query.filter_by(status='active').order_by(
        Employee.full_name).all()

    return render_template('leaves/index.html', leaves=leaves,
                           employees=employees, status=status, emp_id=emp_id)


@leaves_bp.route('/them', methods=['GET', 'POST'])
def create():
    employees = Employee.query.filter_by(status='active').order_by(
        Employee.full_name).all()

    if request.method == 'POST':
        employee_id = request.form.get('employee_id')
        leave_type = request.form.get('leave_type', '').strip()
        start_date_str = request.form.get('start_date', '').strip()
        end_date_str = request.form.get('end_date', '').strip()
        reason = request.form.get('reason', '').strip()

        errors = []
        if not employee_id:
            errors.append('Vui lòng chọn nhân viên.')
        if not leave_type:
            errors.append('Vui lòng chọn loại nghỉ phép.')
        if not start_date_str or not end_date_str:
            errors.append('Vui lòng nhập ngày bắt đầu và kết thúc.')

        if errors:
            for e in errors:
                flash(e, 'danger')
            return render_template('leaves/form.html', employees=employees,
                                   form_data=request.form)

        try:
            start_date = date.fromisoformat(start_date_str)
            end_date = date.fromisoformat(end_date_str)
        except ValueError:
            flash('Ngày không hợp lệ.', 'danger')
            return render_template('leaves/form.html', employees=employees,
                                   form_data=request.form)

        if end_date < start_date:
            flash('Ngày kết thúc phải sau ngày bắt đầu.', 'danger')
            return render_template('leaves/form.html', employees=employees,
                                   form_data=request.form)

        leave = LeaveRequest(
            employee_id=int(employee_id),
            leave_type=leave_type,
            start_date=start_date,
            end_date=end_date,
            reason=reason,
            status='pending',
        )
        db.session.add(leave)
        db.session.commit()
        flash('Tạo đơn nghỉ phép thành công!', 'success')
        return redirect(url_for('leaves.index'))

    return render_template('leaves/form.html', employees=employees, form_data=None)


@leaves_bp.route('/<int:leave_id>/duyet', methods=['POST'])
def approve(leave_id):
    leave = LeaveRequest.query.get_or_404(leave_id)
    action = request.form.get('action', '')
    approved_by = request.form.get('approved_by', 'Quản lý').strip()

    if action == 'approve':
        leave.status = 'approved'
        leave.approved_by = approved_by
        flash('Đã duyệt đơn nghỉ phép.', 'success')
    elif action == 'reject':
        leave.status = 'rejected'
        leave.approved_by = approved_by
        flash('Đã từ chối đơn nghỉ phép.', 'warning')

    db.session.commit()
    return redirect(url_for('leaves.index'))


@leaves_bp.route('/<int:leave_id>/xoa', methods=['POST'])
def delete(leave_id):
    leave = LeaveRequest.query.get_or_404(leave_id)
    db.session.delete(leave)
    db.session.commit()
    flash('Đã xóa đơn nghỉ phép.', 'success')
    return redirect(url_for('leaves.index'))
